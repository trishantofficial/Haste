using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Haste.Exceptions;
using Haste.Interfaces;
using static System.Console;

namespace Haste
{
    public class DownloadClient : IDownloader
    {
        #region Properties

        private static readonly object FileLocker = new object();
        object _statusLocker = new object();
        private DownloadStatus _status;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the object of the download client.
        /// </summary>
        /// <param name="absoluteUri"></param>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        public DownloadClient(string absoluteUri, int startPoint, long endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            UniformResourecIndicator = new Uri(absoluteUri, UriKind.Absolute);
            // This is kind of redundant. But need to be here because its a part of the Interface.
            IsRangeSupported = true;

            _status = DownloadStatus.Initialized;
            Debug.WriteLine("Download status of client: Initialized");
        }

        #endregion

        #region Interface Implementations

        #region Properties

        public Uri UniformResourecIndicator { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public string DownloadPath { get; set; }
        public long TotalSize { get; set; }
        public long StartPoint { get; set; }
        public long EndPoint { get; set; }
        public long DownloadedSize { get; set; }

        public DownloadStatus Status
        {
            get { return _status; }
            private set
            {
                lock (_statusLocker)
                {
                    _status = value;
                }
            }
        }
        public bool IsRangeSupported { get; set; }
        public bool HasChecked { get; set; } 

        #endregion

        #region Methods

        public void CheckUrl()
        {
            DownloadAids.CheckUrl(this);
        }
        
        public void BeginDownload()
        {
            if(Status != DownloadStatus.Initialized)
                throw new NotInitializedException("Download not initialized");
            Status = DownloadStatus.Waiting;

            // Start the download for the individual part.
            DownloadInternal();

        }

        #endregion

        #endregion

        #region Methods

        private void DownloadInternal()
        {
            if(Status != DownloadStatus.Waiting)
                return;
            HttpWebRequest webRequest;
            HttpWebResponse webResponse = null;
            Stream responseStream = null;
            MemoryStream downloadCache = null;

	        try
	        {
		        if (!HasChecked)
		        {
			        CheckUrlAndFile();
		        }

                // Ensure whether the properties are valid.
		        EnsureValidProperty();
                
		        Status = DownloadStatus.Downloading;
                Debug.WriteLine("Download client status: Downloading");
                Debug.WriteLine("Client Id: {0}", ToString());

                // Initialize the request.
		        webRequest = DownloadAids.InitializeWebRequest(this);

                // This will execute in case Address-Range is supported.
		        if (EndPoint != int.MaxValue)
			        webRequest.AddRange(StartPoint + DownloadedSize, EndPoint);
		        else
			        webRequest.AddRange(StartPoint + DownloadedSize);

		        webResponse = webRequest.GetResponse() as HttpWebResponse;

		        if (webResponse != null)
			        responseStream = webResponse.GetResponseStream();

                // HACK the below 2 sizes depend a lot on the download size. Find a way to adjust the sizes according to the download size.
                // Initialize with 1MB of capacity
                downloadCache = new MemoryStream(256);
		        // Initialize with a buffer size.
		        byte[] downloadBuffer = new byte[1*1024];
	            int cachedSize = 0;
	            while (true)
		        {
		            int bytesSize;
		            if (responseStream != null)
                        // Almost always will be 1024. Except last part of the download.
				        bytesSize = responseStream.Read(downloadBuffer, 0, downloadBuffer.Length);
			        else
				        throw new NoResponseReceivedException("Response stream was found out to be null");
			        if (Status != DownloadStatus.Downloading || bytesSize == 0 || 2097152 < (cachedSize + bytesSize))
			        {
				        try
				        {
                            // Once we've either received the full an MB of data -> write to file.
                            // Or if we have reached the last part of the download.
					        WriteCacheToFile(downloadCache, cachedSize);
					        DownloadedSize += cachedSize;
					        if (Status != DownloadStatus.Downloading || bytesSize == 0)
						        break;
                            // Move the pointer to the current location for further write.
				            downloadCache.Seek(0, SeekOrigin.Begin);
				            cachedSize = 0;
				        }
				        catch (Exception e)
				        {
					        WriteLine(e.Message);
					        Debug.WriteLine(e.StackTrace);
				        }
			        }
                    // Keep writing to downloadCahce to later write this to the file.
                    downloadCache.Write(downloadBuffer, 0, bytesSize);
                    // This is to keep count of how much data to write.
                    cachedSize += bytesSize;
                }
	        }
	        catch (Exception e)
	        {
		        WriteLine(e.Message);
		        Debug.WriteLine(e.StackTrace);
	        }
	        finally
	        {
				// Close all the streams and resources.
		        responseStream?.Close();
		        webResponse?.Close();
		        downloadCache?.Close();
	        }
        }

        /// <summary>
        /// Writes the data to a local file.
        /// </summary>
        /// <param name="downloadCache"></param>
        /// <param name="cachedSize"></param>
        private void WriteCacheToFile(MemoryStream downloadCache, int cachedSize)
        {
            lock (FileLocker)
            {
                using (FileStream fileStream = new FileStream(DownloadPath, FileMode.Open))
                {
                    byte[] cacheContent = new byte[cachedSize];
                    downloadCache.Seek(0, SeekOrigin.Begin);
                    downloadCache.Read(cacheContent, 0, cachedSize);
                    fileStream.Seek(DownloadedSize + StartPoint, SeekOrigin.Begin);
                    fileStream.Write(cacheContent, 0, cachedSize);
                }
            }
        }

        /// <summary>
        /// Ensures that the properties associated with the object are valid.
        /// </summary>
        private void EnsureValidProperty()
        {
            if (StartPoint < 0)
                throw new ArgumentOutOfRangeException($"Start point cannot be less than 0");
            if (EndPoint < StartPoint)
                throw new ArgumentOutOfRangeException($"End point cannot be less than startpoint");
        }

        /// <summary>
        /// Checks the url. Sets associated parameters.
        /// Checks if the file exists. Creates it otherwise.
        /// </summary>
        private void CheckUrlAndFile()
        {
            CheckUrl();
            CheckFileOrCreateFile();
        }

        /// <summary>
        /// Checks if the file exists.
        /// Else creates a temporary file with the space and name.
        /// </summary>
        private void CheckFileOrCreateFile()
        {
            DownloadAids.CheckFileOrCreateFile(this, FileLocker);
        }

        #endregion

        #region Overriden Members

        public override string ToString()
        {
            string a =
                string.Format("File name:" + FileName + "\nDownload Path:" + DownloadPath + "\nStart point: " +
                              StartPoint + "\nEnd point: " + EndPoint + "\nStatus: " + Status + "\nIs Checked: " +
                              HasChecked + "\nURI: " + UniformResourecIndicator+"\n");
            return a;
        }

        #endregion
    }
}