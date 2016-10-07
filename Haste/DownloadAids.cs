using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Haste.Exceptions;
using Haste.Interfaces;

namespace Haste
{
    static class DownloadAids
    {
        /// <summary>
        /// Checks if the url is valid and sets associated properties with it.
        /// </summary>
        /// <param name="downloader"></param>
        /// <returns></returns>
        public static void CheckUrl(IDownloader downloader )
        {

            HttpWebRequest webRequest = InitializeWebRequest(downloader);
            using (WebResponse webResponse = webRequest.GetResponse())
            {
                if (webResponse.Headers.Get("Accept-Ranges") != null)
                {
                    downloader.IsRangeSupported = true;
                    Debug.WriteLine("Range supported: {0}", downloader.IsRangeSupported);
                }
                downloader.FileName = Uri.UnescapeDataString(downloader.Url.Split('/').Last());
                downloader.TotalSize = webResponse.ContentLength;
                if(downloader.TotalSize <= 0)
                    throw new SizeTooSmallException("File size = 0. Cannot be downloaded");
                if (!downloader.IsRangeSupported)
                {
                    downloader.StartPoint = 0;
                    downloader.EndPoint = int.MaxValue;
                }
            }
        }

        /// <summary>
        /// Used to initalize the HttpWebRequest.
        /// It is a simple wrapper around the WebRequest.Create method.
        /// </summary>
        /// <param name="downloader"></param>
        /// <returns></returns>
        public static HttpWebRequest InitializeWebRequest(IDownloader downloader)
        {
            HttpWebRequest webRequest = WebRequest.Create(downloader.UniformResourecIndicator) as HttpWebRequest;
            return webRequest;
        }

        /// <summary>
        /// Checks if the file exists.
        /// Else creates a temporary file with the space and name.
        /// </summary>
        /// <param name="downloader"></param>
        /// <param name="locker"></param>
        public static void CheckFileOrCreateFile(IDownloader downloader, object locker)
        {
            lock (locker)
            {
                FileInfo fileToDownload = new FileInfo(downloader.DownloadPath);
                if (fileToDownload.Exists)
                {
                    if (fileToDownload.Length != downloader.TotalSize)
                        throw new Exception("THe download file does not match");
                }
                else
                {
                    if (downloader.TotalSize == 0)
                        throw new FileNotFoundException();
                    using (FileStream fileStream= File.Create(downloader.DownloadPath))
                    {
                        long createdSize = 0;
                        byte [] buffer = new byte[4096];
                        while (createdSize < downloader.TotalSize)
                        {
                            int bufferSize = (downloader.TotalSize - createdSize) < 4096 ? (int)(downloader.TotalSize - createdSize) : 4096;
                            fileStream.Write(buffer, 0, bufferSize);
                            createdSize += bufferSize;
                        }
                    }
                }
            }
        }
    }
}
