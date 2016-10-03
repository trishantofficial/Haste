using System;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Haste
{
    public class Download
    {
        #region Variable and Properties.

        public List<HasteFile> DownloadFile { get; }

        private Task _downloadTask;

        #endregion

        public Download(string url, int partitions)
        {
            DownloadFile = new List<HasteFile>();
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            string name = GetFileName(url);
            long downloadSize = GetDownloadSize(request.GetResponse());
            Console.WriteLine("Download Size: {0}", downloadSize);
            long partitionSize = downloadSize/partitions;
            long partLeft = downloadSize%partitions;
            Console.WriteLine("Part Left: {0}", partLeft);
            Console.WriteLine("Partition size: {0}", partitionSize);
            for (long partNumber = 1, startRange = 0; (int)partNumber <= partitions; partNumber++, startRange += partitionSize)
            {
                string fileName = string.Concat(name, ".", partNumber);
                long endRange = startRange + partitionSize;
                if (startRange != 0)
                    if (endRange + partLeft >= downloadSize)
                        DownloadFile.Add(new HasteFile(url, fileName, (int)partNumber, startRange + 1, endRange + partLeft));
                    else
                        DownloadFile.Add(new HasteFile(url, fileName, (int)partNumber, startRange + 1, endRange));
                else
                    DownloadFile.Add(new HasteFile(url, fileName, (int)partNumber, startRange, endRange));
            }
            foreach(HasteFile hasteFile in DownloadFile)
            {
                long x = hasteFile.EndRange - hasteFile.StartRange;
                Console.WriteLine("Name: {0}",hasteFile.Name);
                Console.WriteLine("Part number: {0}", hasteFile.PartNumber);
                Console.WriteLine("Start range: {0}", hasteFile.StartRange);
                Console.WriteLine("End range: {0}", hasteFile.EndRange);
                Console.WriteLine("Part Difference: {0}", x);
            }
            Console.WriteLine("Start Download? Y/N");
            string dlStart = Console.ReadLine();
            if (dlStart == "y" || dlStart == "Y")
            {
                Console.WriteLine("Download started.");
                
                foreach (var downloadFile in DownloadFile)
                {
                    _downloadTask = Task.Factory.StartNew(() =>
                    {
                        downloadFile.DownloadFile();
                    });
                }

                Task.WaitAll(new Task[]
                {
                    _downloadTask
                });

                Console.WriteLine("Download complete");
            }
            Console.WriteLine("Compile Files? Y/N");
            string compileChar = Console.ReadLine();
            if (compileChar == "y" || compileChar == "Y")
            {
                Console.WriteLine("Compiling Files.....");
                FileCompiler(name, DownloadFile);
            }
        }
        public string GetFileName(string hrefLink)
        {
            Uri uri = new Uri(hrefLink);
            string fileName = System.IO.Path.GetFileName(uri.LocalPath);
            return fileName;
        }
        public long GetDownloadSize(WebResponse response)
        {
            long size = 0;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    size = response.ContentLength;
                }
                catch (Exception)
                {
                    Console.WriteLine("Error connecting...");
                    Console.WriteLine("Trying again!");
                }
            }
            if (size == 0)
            {
                Console.WriteLine("HasteFile cannot be downloaded.");
                Environment.Exit(0);
            }
            else
            {
                return size;
            }
            return -1;
        }
        public void FileCompiler(string fileName, List<HasteFile> downloadFiles)
        {
            CompiledFile compiledFile = new CompiledFile(fileName, downloadFiles);
            compiledFile.Compile();
        }
    }
}