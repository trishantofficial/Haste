using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            Tuple<long, Boolean> downloadProperties = getDownloadProperites(URL);
            long downloadSize = downloadProperties.Item1;
            Boolean downloadResumable = downloadProperties.Item2;
            Console.WriteLine("File size = " + downloadSize);
            if (!downloadResumable)
            {
                 partitions = 1;  
            }
            long partitionSize = downloadSize / partitions;
            long partLeft = downloadSize % partitions;
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
                ServicePointManager.Expect100Continue = false;
                ServicePointManager.DefaultConnectionLimit = 20;
                /*var downloadStatus = new List<ManualResetEvent>();
                int cores = Environment.ProcessorCount * 2;
                ThreadPool.SetMaxThreads(cores, cores);
                foreach (var downloadFile in downloadFiles)
                {
                    var downloadPartStatus = new ManualResetEvent(false);
                    ThreadPool.QueueUserWorkItem(
                        arg =>
                        {
                            downloadFile.downloadFile();
                            downloadPartStatus.Set();
                        });
                    downloadStatus.Add(downloadPartStatus);
                }
                WaitHandle.WaitAll(downloadStatus.ToArray());*/
                Parallel.ForEach(downloadFiles, (downloadFile) => downloadFile.downloadFile());
                Console.WriteLine("Download complete");
            }
            Console.WriteLine("Compile Files? Y/N");
            String compileChar = Console.ReadLine();
            if (compileChar == "y" || compileChar == "Y")
            {
                Console.WriteLine("Compiling Files.....");
                fileCompiler(name, downloadFiles);
            }
        }
        public String getFileName(String hrefLink)
        {
            Uri uri = new Uri(hrefLink);
            string fileName = System.IO.Path.GetFileName(uri.LocalPath);
            return fileName;
        }
        public Tuple<long, Boolean> getDownloadProperites(String URL)
        {
            Boolean resumable = false;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "HEAD";
            request.Timeout = 36000;
            long size = -1;
            short responseCode = 0;
            for (int retry = 3; retry >= 0; retry--) {
                try
                {
                    using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                    {
                        size = response.ContentLength;
                        responseCode = (short) response.StatusCode;
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
            return new Tuple<long, Boolean>(size, resumable);
        }
        public void fileCompiler(String fileName, List<Haste.File> downloadFiles)
        {
            Haste.compiledFile CompiledFile = new Haste.compiledFile(fileName, downloadFiles);
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