using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _26_6_10_메모화
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Fibonacci.Get(40));
            Console.WriteLine(Fibonacci.Get(100));
        }
    }
}
