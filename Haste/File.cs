using System;
using System.Net;
using System.IO;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haste
{
    public class File
    {
        private String name, URL;
        private Boolean isPart;
        private int partNumber;
        private long startRange, endRange;
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
            Console.WriteLine("Downloading file " + this.getPartNumber());
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.AddRange((long)this.startRange, (long)this.endRange);
            request.Method = "HEAD";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            long responseLength = long.Parse(response.Headers.Get("Content-Length"));
            Console.WriteLine("");

            /*WebClient webClient = new WebClient();
            //webClient.DownloadProgressChanged += webClient_DownloadProgressChanged;
            String rangeString = "bytes=" + (long)this.startRange + "-" + (long)this.endRange;
            webClient.Headers["Range"] = rangeString;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    webClient.DownloadFileAsync(new Uri(this.URL), this.name);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error downloading File...");
                    Console.WriteLine("Retrying.... " + i + "th time");
                }
            }*/
        }
        /*void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }*/
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