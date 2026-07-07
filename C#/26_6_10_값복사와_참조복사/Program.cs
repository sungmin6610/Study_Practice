using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _26_6_10_값복사와_참조복사
{
    class Program
    {
        class Test
        {
            public int value = 10;
        }

        static void Change(Test input)
        {
            input.value = 20;
        }

        static void Main(string[] args)
        {
            Test test = new Test();
            test.value = 10;
            Change(test);

            Console.WriteLine(test.value);
        }
    }
}
