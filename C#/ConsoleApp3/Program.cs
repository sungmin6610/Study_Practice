using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    class FileName
    {
        static void Main(string[] args)
        {
            //int a = 273;
            //int b = 52;
            //Console.WriteLine(a + b);
            //Console.WriteLine(a - b);
            //Console.WriteLine(a * b);
            //Console.WriteLine(a / b);
            //Console.WriteLine(a % b);
            //Console.WriteLine(a + b);


            //int c = 2147483647;
            //int d = 1;
            //Console.WriteLine(c + d);
            //Console.WriteLine(int.MaxValue);
            //Console.WriteLine(int.Minvalue);
            //Console.WriteLine(long.MaxValue);
            //Console.WriteLine(long.Minvalue);


            //Console.WriteLine("int: " + sizeof(int));
            //Console.WriteLine("long: " + sizeof(long));
            //Console.WriteLine("float: " + sizeof(float));
            //Console.WriteLine("double: " + sizeof(double));
            //Console.WriteLine("char: " + sizeof(char));
            //Console.WriteLine("string" + sizeof(String));


            //Console.WriteLine();
            //bool one = 10 < 0;
            //bool other = 20 > 100;
            //Console.WriteLine(one);
            //Console.WriteLine(other);


            //int number = 10;
            //Console.WriteLine(number++);
            //Console.WriteLine(++number);
            //Console.WriteLine(number--);

            int _int = 273;
            long _long = 522731033265;
            float _float = 52.273f;
            double _double = 52.373;
            string _string = "문자열";
            char _char = '글';
            Console.WriteLine(_int.GetType());
            Console.WriteLine(_long.GetType());
            Console.WriteLine(_float.GetType());
            Console.WriteLine(_double.GetType());
            Console.WriteLine(_char.GetType());
            Console.WriteLine(_string.GetType());
        }
    }
}