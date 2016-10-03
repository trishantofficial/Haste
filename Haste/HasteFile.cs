using System;
using System.Net;
using System.IO;

namespace Haste
{
    public class HasteFile
    {
        public string Name { get; }
        public string Url { get; }
        public bool IsPart { get; private set; }
        public int PartNumber { get; }
        public long StartRange { get; }
        public long EndRange { get; }

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
                using (
                    FileStream downloadStream = new FileStream(Name, FileMode.Create, FileAccess.Write,
                        FileShare.None))
                {
                    HttpWebRequest httpRequest = WebRequest.Create(Url) as HttpWebRequest;
                    httpRequest.AddRange(StartRange, EndRange);
                    httpRequest.Timeout = 3600000;
                    using (HttpWebResponse httpResponse = (HttpWebResponse) httpRequest.GetResponse())
                    {
                        using (Stream responseStream = httpResponse.GetResponseStream())
                        {
                            int bytesRead = 0;
                            byte[] buffer = new byte[4096];
                            while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                downloadStream.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
            Console.WriteLine("HasteFile " + PartNumber + " downloaded.");
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