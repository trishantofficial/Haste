using System;
using System.Net;
using System.IO;
using System.ComponentModel;

namespace Haste
{
    // TODO wouldnt be needing this either.
    public class HasteFile
    {
        #region Variables and Properties

        public string Name { get; private set; }

        public string Url { get; private set; }

        public bool IsPart { get; private set; }

        public int PartNumber { get; private set; }

        public long StartRange { get; private set; }

        public long EndRange { get; private set; }

        public EventHandler<ProgressChangedEventArgs> Progress;

        public ProgressChangedEventArgs Percentage { get; set; } 

        #endregion

        public HasteFile(string url, string name, int partNumber, long startRange, long endRange)
        {
            Url = url;
            Name = name;
            IsPart = true;
            PartNumber = partNumber;
            StartRange = startRange;
            EndRange = endRange;
        }

        public void DownloadFile()
        {
            try
            {
                Console.WriteLine("Downloading file " + PartNumber);
                using (FileStream downloadStream = new FileStream(Name, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    HttpWebRequest httpRequest = WebRequest.Create(Url) as HttpWebRequest;
                    if (httpRequest != null)
                    {
                        httpRequest.AddRange(StartRange, EndRange);
                        httpRequest.Proxy = null;
                        httpRequest.Timeout = 3600000;
                        using (HttpWebResponse httpResponse = (HttpWebResponse) httpRequest.GetResponse())
                        {
                            using (Stream responseStream = httpResponse.GetResponseStream())
                            {
                                int bytesRead;
                                byte[] buffer = new byte[65300];
                                while (responseStream != null && (bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    //int percentage = (int) (fileRead / totalBytes);
                                    //Percentage = new ProgressChangedEventArgs(percentage);
                                    //Console.WriteLine("File " + partNumber + " bytes read = " + bytesRead);
                                    downloadStream.Write(buffer, 0, bytesRead);
                                }
                            }
                        }
                    }
                }
            Console.WriteLine("File " + PartNumber + " downloaded.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't download file {0}", PartNumber);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Retrying download {0}", PartNumber);
                DownloadFile();
            }
        }
    }
}