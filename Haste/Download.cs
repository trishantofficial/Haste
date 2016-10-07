using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Haste.Exceptions;
using Haste.Interfaces;
using static System.Console;

namespace Haste
{
    public class Download : IDownloader
    {
        #region Variable and Properties

        private List<DownloadClient> _downloadClients = new List<DownloadClient>();

        public int Partitions { get; private set; }


        public int MaxThreadCount { get; set; }

        private Task _downloadTask, _clientDownloadTask;

        private List<Task> _listOfDownloadTasks = new List<Task>();

        private static readonly object Locker = new object();

        #region Interface Variables

        public Uri UniformResourecIndicator { get; set; }
        public string Url { get; set; }

        public string FileName { get; set; }

        public string DownloadPath { get; set; }
        public long TotalSize { get; set; }
        public long StartPoint { get; set; }
        public long EndPoint { get; set; }
        public long DownloadedSize { get; set; }
        public DownloadStatus Status { get; private set; }
        public bool IsRangeSupported { get; set; }
        public bool HasChecked { get; set; }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Download class.
        /// Helps us to download the file.
        /// </summary>
        /// <param name="url"></param>
        public Download(string url) : this(url, Environment.ProcessorCount * 2)
        {
            // Simply calls the other constructor.
        }

        /// <summary>
        /// Actually initializes stuff.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="threadCount"></param>
        private Download(string url, int threadCount)
        {
            // Initialize all the variables.
            Url = url;
            UniformResourecIndicator = new Uri(url);
            // Set the MaxThreadCount and number of partitions.
            Partitions = threadCount;
            MaxThreadCount = Partitions;

            WriteLine("We have choosen the ideal number of threads to be equal to: {0}", threadCount);
            WriteLine("Would you like to override it?");
            // To make it stand out.
            ForegroundColor = ConsoleColor.Red;
            WriteLine("Caution! This may cause poor performance");
            // Reset it back to what it was.
            ForegroundColor = ConsoleColor.Gray;
            WriteLine("1.Yes\n2.No");
            int choice = int.Parse(ReadLine());
            if (choice == 1)
            {
                Write("Enter the number of part: ");
                Partitions = int.Parse(ReadLine());
                MaxThreadCount = Partitions;
            }

            // Set the connection properties
            ServicePointManager.DefaultConnectionLimit = MaxThreadCount;
            // Btw do we need this... I am still not sure why this is needed. Delete the comment if you feel that this is needed.
            ServicePointManager.Expect100Continue = false;

            // Initialize the download status.
            Status = DownloadStatus.Initialized;
            Debug.WriteLine("Download status: Initialized");
        }

        #endregion

        #region Interface Members

        /// <summary>
        /// Checks if the url is valid and sets associated properties with it.
        /// </summary>
        public void CheckUrl()
        {
            // Pass it the current object as it need to fiddle with its properties as well.
            DownloadAids.CheckUrl(this);
        }

        /// <summary>
        /// Starts the process of downloading.
        /// </summary>
        public void BeginDownload()
        {
            WriteLine("Download started.");
            if(Status != DownloadStatus.Initialized)
                throw new NotInitializedException("Download not initialized");

            Status = DownloadStatus.Waiting;
            Debug.WriteLine("Download status: Waiting");

            // Start the download on a separate thread.
            _downloadTask = Task.Factory.StartNew(DownloadInternal);
            Debug.WriteLine("Started the task to download.");
            Debug.WriteLine("Waiting for the task to complete.");

            // Wait for the downloads to finish.
            Task.WaitAll(new Task []
            {
                _downloadTask
            });

            // Set the status to completed.
            Status = DownloadStatus.Completed;
            Debug.WriteLine("Download status: Completed.");

        }

        private void DownloadInternal()
        {
            if(Status != DownloadStatus.Waiting)
                return;
            try
            {
                EnsureValidProperty();
                Debug.WriteLine("Properties valid!");

                Status = DownloadStatus.Downloading;
                Debug.WriteLine("Download Status: Downloading");

                if (!HasChecked)
                {
                    CheckUrlAndFile();
                }

                // If range is not supported then add 1 client to the download list.
                if (!IsRangeSupported)
                {
                    DownloadClient client = new DownloadClient(UniformResourecIndicator.AbsoluteUri, 0, long.MaxValue);
                    client.TotalSize = TotalSize;
                    client.DownloadPath = DownloadPath;
                    client.HasChecked = true;

                    _downloadClients.Add(client);
                    Debug.WriteLine("Added a client with following parameters:\n{0}", client.ToString());
                }

                // Else add multiple connections to the list.
                else
                {
                    // Bytes each client will download.
                    int maxSizePerThread = (int) Math.Ceiling((double) TotalSize/MaxThreadCount);

                    for (int i = 0; i < MaxThreadCount; i++)
                    {
                        long endPoint = maxSizePerThread*(i + 1) - 1;
                        long sizeToDownload = maxSizePerThread;

                        // This is just for the last part. Kind of like a safety check.
                        if (endPoint > TotalSize)
                        {
                            endPoint = TotalSize - 1;
                            sizeToDownload = endPoint - maxSizePerThread*i;
                        }

                        // Initialize the client with the associated parameters.
                        DownloadClient client = new DownloadClient(UniformResourecIndicator.AbsoluteUri,
                            maxSizePerThread*i, endPoint);
                        client.DownloadPath = DownloadPath;
                        client.TotalSize = sizeToDownload;
                        client.HasChecked = true;

                        _downloadClients.Add(client);
                        Debug.WriteLine("Added a client with following parameters:\n{0}", client.ToString());
                    }
                }

                foreach (DownloadClient client in _downloadClients)
                {
                    // Start the download of each client.
                    _clientDownloadTask = Task.Factory.StartNew(client.BeginDownload);
                    _listOfDownloadTasks.Add(_clientDownloadTask);
                }
                Debug.WriteLine("Started download of all clients.");

                // Wait for the list of clients to finish downloading.
                Task.WaitAll(_listOfDownloadTasks.ToArray());
                Debug.WriteLine("All downloads have completed.");
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

        #endregion

        #region Class Methods


        /// <summary>
        /// Checks the url. Sets associated parameters.
        /// Checks if the file exists. Creates it otherwise.
        /// </summary>
        private void CheckUrlAndFile()
        {
            CheckUrl();
            CheckFileOrCreateFile();

            // Passes the tests. Thus set the parameter to be true.
            HasChecked = true;
            Debug.WriteLine("Has checked: {0}", HasChecked);
        }

        /// <summary>
        /// Checks if the file exists.
        /// Else creates a temporary file with the space and name.
        /// </summary>
        private void CheckFileOrCreateFile()
        {
            DownloadAids.CheckFileOrCreateFile(this, Locker);
        }

        /// <summary>
        /// Ensure that the properties of the download are correct.
        /// </summary>
        private void EnsureValidProperty()
        {
            if(StartPoint < 0)
                throw new ArgumentOutOfRangeException($"Start point cannot be less than 0");
            if (EndPoint < StartPoint)
                throw new ArgumentOutOfRangeException($"End point cannot be less than startpoint");
            if (MaxThreadCount < 1)
                throw new ArgumentOutOfRangeException($"Number of threads cannot be less than 1");
            Debug.WriteLine("Properties are valid!");
        }

        #endregion
    }
}