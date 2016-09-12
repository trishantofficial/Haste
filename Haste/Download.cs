using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using Haste;

namespace Haste
{
    public class Download
    {
        private List<Haste.File> downloadFiles = new List<Haste.File>();
        private int partitions;
        private String URL;
        public Download(String URL, int partitions)
        {
            String name = getFileName(URL);
            long downloadSize = getDownloadSize(URL);
            Console.WriteLine(downloadSize);
            long partitionSize = downloadSize / (long)partitions;
            long partLeft = downloadSize % (long)partitions;
            Console.WriteLine(partLeft);
            Console.WriteLine(partitionSize);
            for (long partNumber = 1, startRange = 0; (int)partNumber <= partitions; partNumber++, startRange += partitionSize)
            {
                String fileName = String.Concat(name, ".", partNumber);
                long endRange = startRange + partitionSize;
                if (startRange != 0)
                    if (endRange + partLeft >= downloadSize)
                        downloadFiles.Add(new Haste.File(URL, fileName, (int)partNumber, startRange + 1, endRange + partLeft, true));
                    else
                        downloadFiles.Add(new Haste.File(URL, fileName, (int)partNumber, startRange + 1, endRange, true));
                else
                    downloadFiles.Add(new Haste.File(URL, fileName, (int)partNumber, startRange, endRange, true));
            }
            foreach(var downloadFile in downloadFiles)
            {
                long x = downloadFile.getEndRange() - downloadFile.getStartRange();
                Console.WriteLine("Name: " + downloadFile.getName() + "\nPart number: " + downloadFile.getPartNumber() + "\n Start Range:" + downloadFile.getStartRange() + "\n End Range:" + downloadFile.getEndRange() + "\nPart Difference = " + x + "\n\n");
            }
            Console.WriteLine("Start Download? Y/N");
            String dlStart = Console.ReadLine();
            if (dlStart == "y" || dlStart == "Y")
            {
                Console.WriteLine("Download started.");
                foreach (var downloadFile in downloadFiles)
                {
                    downloadFile.downloadFile();
                }
            }
            Console.WriteLine("Compile Files? Y/N");
            String compileChar = Console.ReadLine();
            if (compileChar == "y" || compileChar == "Y")
            {
                Console.WriteLine("Start Download? Y/N");
                fileCompiler(name, downloadFiles);
            }
        }
        public String getFileName(String hrefLink)
        {
            Uri uri = new Uri(hrefLink);
            string fileName = System.IO.Path.GetFileName(uri.LocalPath);
            return fileName;
        }
        public long getDownloadSize(String URL)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "HEAD";
            long size = 0;
            for (int retry = 3; retry >= 0; retry--) {
                try
                {
                    HttpWebResponse response = (HttpWebResponse)(request.GetResponse());
                    size = response.ContentLength;
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error connecting...");
                    Console.WriteLine("Retrying....");
                }
            }
            if (size == 0)
            {
                Console.WriteLine("File cannot be downloaded.");
                Thread.Sleep(5000);
                Environment.Exit(0);
                return size;
            }
            else
            {
                return size;
            }
        }
        public void fileCompiler(String fileName, List<Haste.File> downloadFiles)
        {
            Haste.compiledFile CompiledFile = new Haste.compiledFile(fileName, downloadFiles);
            CompiledFile.createCompiledFile();
            CompiledFile.compile();
        }
        public void setDownloadFiles(List<Haste.File> downloadFiles)
        {
            this.downloadFiles = downloadFiles;
        }
        public List<Haste.File> getDownloadFiles()
        {
            return this.downloadFiles;
        }
        public void setPartitions(int partitions)
        {
            this.partitions = partitions;
        }
        public int getPartitions()
        {
            return this.partitions;
        }
        public void setURL(String URL)
        {
            this.URL = URL;
        }
        public String getURL()
        {
            return this.URL;
        }
    }
}