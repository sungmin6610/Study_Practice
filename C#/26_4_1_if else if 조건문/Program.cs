using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp6
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.Write("학점 입력: ");
            double score = double.Parse(Console.ReadLine());
            if (score == 4.5)
                Console.WriteLine("a+");
            else if (4.2 <= score && score < 4.5)
                Console.WriteLine("a");
            else if (3.5 <= score && score < 4.2)
                Console.WriteLine("b+");
            else if (2.8 <= score && score < 3.5)
                Console.WriteLine("b");
            else if (2.3 <= score && score < 2.8)
                Console.WriteLine("c+");
            else if (1.75 <= score && score < 2.3)
                Console.WriteLine("c");
            else if (1.0 <= score && score < 1.74)
                Console.WriteLine("d+");
            else if (0.5 <= score && score < 1.0)
                Console.WriteLine("d");
            else
                Console.WriteLine("학교 왜다님?");

        }
    }
}
