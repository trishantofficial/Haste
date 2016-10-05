using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Haste
{
    public class Download
    {
        #region Variable and Properties

        private List<HasteFile> _downloadFiles = new List<HasteFile>();

        public int Partitions { get; private set; }

        public string Url { get; private set; }
        
        public string FileName { get; private set; }

        public long PartitionSize { get; private set; }

        public long PartLeft { get; private set; }

        #endregion


        /// <summary>
        /// Initializes a new instance of the Download class.
        /// Helps us to download the file.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="partitions"></param>
        public Download(string url, int partitions)
        {
            Url = url;
            FileName = Uri.UnescapeDataString(Url.Split('/').Last());
            // Item1: Download Size
            // Item2: Download Resumable
            Tuple<long, bool> downloadProperties = GetDownloadProperites(url);
            Console.WriteLine("File size = " + downloadProperties.Item1);
            if (!downloadProperties.Item2)
            {
                Partitions = 1;
            }
            PartitionSize = downloadProperties.Item1 / partitions;
            PartLeft = downloadProperties.Item1 % partitions;
            Console.WriteLine(PartLeft);
            Console.WriteLine(PartitionSize);

            for (long partNumber = 1, startRange = 0; (int)partNumber <= partitions; partNumber++, startRange += PartitionSize)
            {
                string fileName = string.Concat(FileName, ".", partNumber);
                long endRange = startRange + PartitionSize;
                if (startRange != 0)
                    if (endRange + PartLeft >= downloadProperties.Item1)
                        _downloadFiles.Add(new HasteFile(url, fileName, (int)partNumber, startRange + 1, endRange + PartLeft));
                    else
                        _downloadFiles.Add(new HasteFile(url, fileName, (int)partNumber, startRange + 1, endRange));
                else
                    _downloadFiles.Add(new HasteFile(url, fileName, (int)partNumber, startRange, endRange));
            }
            foreach(var downloadFile in _downloadFiles)
            {
                long x = downloadFile.EndRange - downloadFile.StartRange;
                Console.WriteLine("Name: " + downloadFile.Name + "\nPart number: " + downloadFile.PartNumber + "\n Start Range:" + downloadFile.StartRange + "\n End Range:" + downloadFile.EndRange + "\nPart Difference = " + x + "\n\n");
            }
            BeginDownload();
        }

        /// <summary>
        /// Starts the process of downloading.
        /// </summary>
        private void BeginDownload()
        {
            Console.WriteLine("Start Download? Y/N");
            string dlStart = Console.ReadLine();
            if (dlStart == "y" || dlStart == "Y")
            {
                Console.WriteLine("Download started.");
                ServicePointManager.Expect100Continue = false;
                ServicePointManager.DefaultConnectionLimit = 20;
                var downloadStatus = new List<ManualResetEvent>();
                ThreadPool.SetMaxThreads(4, 4);
                foreach (var downloadFile in _downloadFiles)
                {
                    var downloadPartStatus = new ManualResetEvent(false);
                    ThreadPool.QueueUserWorkItem(
                        arg =>
                        {
                            downloadFile.DownloadFile();
                            downloadPartStatus.Set();
                        });
                    downloadStatus.Add(downloadPartStatus);
                }
                WaitHandle.WaitAll(downloadStatus.ToArray());
                Console.WriteLine("Download complete");
            }
            FileCompiler(FileName, _downloadFiles);
        }

        private Tuple<long, bool> GetDownloadProperites(string URL)
        {
            bool resumable = false;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "HEAD";
            request.Timeout = 36000;
            long size = -1;
            for (int retry = 3; retry >= 0; retry--) {
                try
                {
                    using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                    {
                        size = response.ContentLength;
                        if (response.Headers.Get("Accept-Ranges") != null)
                        {
                            resumable = true;
                        }
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error connecting...");
                    Console.WriteLine("Retrying....");
                    Console.WriteLine(e.Source);
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
            Console.WriteLine("Downloading file.");
            return new Tuple<long, bool>(size, resumable);
        }

        private void FileCompiler(string fileName, List<Haste.HasteFile> downloadFiles)
        {
            CompiledFile compiledFile = new CompiledFile(fileName, downloadFiles);
            compiledFile.Compile();
        }
    }
}