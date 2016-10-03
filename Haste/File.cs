using System;
using System.Net;
using System.IO;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Haste
{
    public class File
    {
        private String name, URL;
        private Boolean isPart;
        private int partNumber;
        private long startRange, endRange;
        public EventHandler<ProgressChangedEventArgs> Progress;
        public File(String URL, String name, int partNumber, long startRange, long endRange, Boolean isPart = true)
        {
            setURL(URL);
            setName(name);
            setIsPart(true);
            setPartNumber(partNumber);
            setStartRange(startRange);
            setEndRange(endRange);
        }

        public void downloadFile()
        {
            try
            {
                Console.WriteLine("Downloading file " + this.getPartNumber());
                using (FileStream downloadStream = new FileStream(this.name, FileMode.Create, FileAccess.Write,
                        FileShare.None))
                {
                    /*HTTPRequest HttpRequest = new HTTPRequest(this.URL, startRange, endRange);
                    HttpRequest.startRequest();
                    HTTPResponse HttpResponse = new HTTPResponse(HttpRequest.getRequestSocket());
                    HttpResponse.readResponse(downloadStream);*/
                    HttpWebRequest HttpRequest = (HttpWebRequest) WebRequest.Create(this.URL) as HttpWebRequest;
                    HttpRequest.AddRange(this.startRange, this.endRange);
                    HttpRequest.Proxy = null;
                    HttpRequest.Timeout = 3600000;
                    using (HttpWebResponse HttpResponse = (HttpWebResponse) HttpRequest.GetResponse())
                    {
                        using (Stream responseStream = HttpResponse.GetResponseStream())
                        {
                            int bytesRead = 0;
                            long fileRead = 0;
                            long totalBytes = HttpResponse.ContentLength;
                            byte[] buffer = new byte[65300];
                            while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fileRead += bytesRead;
                                int percentage = (int) (fileRead / totalBytes);
                                //Percentage = new ProgressChangedEventArgs(percentage);
                                //Console.WriteLine("File " + partNumber + " bytes read = " + bytesRead);
                                downloadStream.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
            Console.WriteLine("File " + this.getPartNumber() + " downloaded.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't download file " + this.getPartNumber());
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Retrying download ", this.getPartNumber());
                downloadFile();
            }
        }

        public ProgressChangedEventArgs Percentage { get; set; }

        public void setURL(String URL)
        {
            this.URL = URL;
        }
        public String getURL()
        {
            return this.URL;
        }
        public void setName(String name)
        {
            this.name = name;
        }
        public String getName()
        {
            return this.name;
        }
        public void setIsPart(Boolean isPart)
        {
            this.isPart = isPart;
        }
        public Boolean getIsPart()
        {
            return this.isPart;
        }
        public void setPartNumber(int partNumber)
        {
            this.partNumber = partNumber;
        }
        public int getPartNumber()
        {
            return this.partNumber;
        }
        public void setStartRange(long startRange)
        {
            this.startRange = startRange;
        }
        public long getStartRange()
        {
            return this.startRange;
        }
        public void setEndRange(long endRange)
        {
            this.endRange = endRange;
        }
        public long getEndRange()
        {
            return this.endRange;
        }
    }
}