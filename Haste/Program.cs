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
            Download x = new Download(URL, 1); /*Downloads in 1 part.... Change 2nd argument for n number of parts.*/
            Console.WriteLine("Program Ended.....");
            Console.ReadLine();
        }
    }
}
