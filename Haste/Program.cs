using System;

namespace Haste
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Program Start");
            Console.WriteLine("Enter URL: ");
            string url = Console.ReadLine();
            Console.WriteLine("Enter number of parts...");
            int parts = int.Parse(Console.ReadLine());
            new Download(url, parts);
            Console.WriteLine("Program Ended.....");
            Console.WriteLine("Press any key to continue!");
            Console.ReadLine();
        }
    }
}
