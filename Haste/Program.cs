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
            String URL = Console.ReadLine();
            Download x = new Download(URL, 7);
            Console.WriteLine("Program Ended.....");
            Console.ReadLine();
        }
    }
}
