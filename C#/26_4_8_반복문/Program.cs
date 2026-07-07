using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _26._4._8_반복문
{
    class Program
    {
        static void Main(string[] args)
        {
            //int[] intArray = { 52, 273, 32, 65, 103 };
            //intArray[0] = 0;

            //Console.WriteLine(intArray[0]);
            //Console.WriteLine(intArray[1]);
            //Console.WriteLine(intArray[2]);
            //Console.WriteLine(intArray[3]);
            //Console.WriteLine(intArray[4]);

            // while 반복문
            //int i = 0;
            //while (i < intArray.Length)
            //{
            //    Console.WriteLine(i + "번째 출력: " + intArray[i]);

            //    i++;
            //}

            //do while 반복문
            //string input;
            //do
            //{
            //    Console.Write("입력(exit을 입력하면 종료): ");
            //    input = Console.ReadLine();
            //} while (input != "exit");

            //for 반복문
            //for (int i = '가'; i <= '힣'; i++)
            //{
            //    Console.Write((char)i);
            //}

            //foreach 반복문
            //string[] array = { "사과", "배", "포도", "딸기", "바나나" };
            //foreach(string item in array)
            //{
            //    Console.WriteLine(item);
            //}

            //중첩 반복문
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < i + 1; j++)
                    Console.Write('*');
                Console.Write('\n');
            }
        }
    }
}
