using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _26_6_10_Box_class
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Box box = new Box(10, 10);

            Console.WriteLine("Box width: " + box.GetWidth());
            Console.WriteLine("Box Height: " + box.GetHeight());
            Console.WriteLine("Area: " + box.Area());
        }
    }
}
