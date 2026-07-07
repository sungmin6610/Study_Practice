using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    class Program
    {
        static void Main(string[] args)
        {
            //long longnumber = 2147483647L + 2147483647L;
            //Console.WriteLine(longnumber);
            //int intNumber = (int)longnumber;
            //Console.WriteLine(intNumber);

            //string numberString = "52273";
            ////int intNumber2 = (int)numberstring;
            //int intNumber = int.Parse(numberString);
            //Console.WriteLine(intNumber);
            //Console.WriteLine();
            //Console.WriteLine(float.Parse("52.273"));

            //Console.WriteLine((777).ToString());
            //Console.WriteLine((777).ToString().GetType());

            unchecked
            {
                Console.WriteLine(-(-2147483648));
            }
            
        }
    }
}
