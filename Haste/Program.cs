using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haste;

namespace Haste
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Program Start");
            Console.WriteLine("Enter URL: ");
            //String URL = "http://www.simpte.ch/ebooks/Networking%20Books/CCNA%20Security.pdf";
            String URL = Console.ReadLine();
            Console.WriteLine("Enter number of parts...");
            int parts = int.Parse(Console.ReadLine());
            var startTime = DateTime.Now;
            Download x = new Download(URL, parts);
            var endTime = DateTime.Now;
            TimeSpan time = endTime - startTime;
            Console.WriteLine("Time taken = " + time.Seconds + " seconds.");
            Console.WriteLine("Program Ended.....");
            Console.ReadLine();
        }
    }
}
