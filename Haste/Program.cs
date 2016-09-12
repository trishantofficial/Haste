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
            String URL = "http://www.ibruce.net/eMag/Playboy/Playboy%20Magazine%20Nederland%20-%20October%202011.pdf";
            Download x = new Download(URL, 7);
            Console.WriteLine("Program Ended.....");
            Console.ReadLine();
        }
    }
}
