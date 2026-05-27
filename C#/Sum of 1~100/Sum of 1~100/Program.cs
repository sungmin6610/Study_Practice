using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sum_of_1_100
{
    class Program
    {
        static void Main(string[]args)
        {
            int sum = 0;
            for (int i = 1; i <= 100; i++)
            {
                sum += i;
              
            }
            Console.WriteLine("1~100 까지 더한 값 : ", sum);
        }
    }
}
