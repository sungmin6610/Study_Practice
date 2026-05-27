using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _26_5_27_메서드
{
    class Test
    {
        public int Power(int x)
        {
            return x * x;
        }

        public int Multi(int x, int y)
        {
            return x * y;
        }
        
        public void Print()
        {
            Console.WriteLine("Print() 메서드가 호출되었습니다.");
        }

        public int Sum(int min, int max)
        {
            int output = 0;
            for(int i = min; i<=max; i++)
            {
                output += i;
            }
            return output;
        }

        public int Multiply(int min, int max)
        {
            int output = 1;
            for(int i = min; i <= max; i++)
            {
                output *= i;
            }
            return output;
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Test test = new Test();
            Console.WriteLine(test.Power(10));
            Console.WriteLine(test.Power(20));
            Console.WriteLine(test.Multi(10, 20));

            test.Print();
            Console.WriteLine(test.Sum(1, 100));
            Console.WriteLine(test.Multiply(1, 10));
        }
    }
}
