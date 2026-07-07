using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2026._4._1_switch조건문
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("숫자를 입력하세요: ");
            //int input = int.Parse(Console.ReadLine());

            //switch (input %2)
            //{
            //    case 0:
            //        Console.WriteLine("짝수입니다.");
            //        Console.WriteLine("짝수입니다.");
            //        break;
            //    case 1:
            //        Console.WriteLine("홀수입니다.");
            //        Console.WriteLine("홀수입니다.");
            //        break;
            //}

            Console.WriteLine("숫자를 입력하세요: ");
            int mon = int.Parse(Console.ReadLine());

            switch (mon)
            {
                case 12:
                case 1:
                case 2:
                    Console.WriteLine("겨울 입니다.");
                    break;
                case 3:
                case 4:
                case 5:
                    Console.WriteLine("봄 입니다.");
                    break;
                case 6:
                case 7:
                case 8:
                    Console.WriteLine("여름 입니다.");
                    break;
                case 9:
                case 10:
                case 11:
                    Console.WriteLine("가을 입니다.");
                    break;

                default:
                    Console.WriteLine("어느 행성에 살고 계신가요?");
                    break;
            }
        }
    }
}
