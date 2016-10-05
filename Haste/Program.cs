using System;
using static System.Console;

namespace Haste
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteLine("Program Start");
            WriteLine("Enter URL: ");
            //string URL = "http://www.simpte.ch/ebooks/Networking%20Books/CCNA%20Security.pdf";
            string URL = Console.ReadLine();
            WriteLine("Number of threads: {0}", Environment.ProcessorCount*2);
            int parts = int.Parse(ReadLine());
            Download x = new Download(URL, parts);
            WriteLine("Program Ended.....");
            ReadLine();
        }
    }
}
