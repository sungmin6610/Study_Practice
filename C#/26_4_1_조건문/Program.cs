using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp5
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("숫자 입력: ");
            //int input = int.Parse(Console.ReadLine());

            //if (input % 2 == 0)
            //{
            //    Console.WriteLine("짝수입니다");
            //}

            //if(input % 2 == 1)
            //{
            //    Console.WriteLine("홀수입니다.");
            //}

            //Console.Write(DateTime.Now.Year+"년 ");
            //Console.Write(DateTime.Now.Month+"월 ");
            //Console.Write(DateTime.Now.Day+"일 ");

            //if (DateTime.Now.Hour < 12)
            //{ 
            //    Console.Write("am");
            //    Console.Write(DateTime.Now.Hour + "시 ");
            //}

            //if (DateTime.Now.Hour >= 12)
            //{ 
            //    Console.Write("pm ");
            //    Console.Write(DateTime.Now.Hour-12 +"시 ");
            //}

            //Console.Write(DateTime.Now.Minute+"분 ");
            //Console.Write(DateTime.Now.Second+"초");

            //Console.WriteLine("숫자 입력: ");
            //int input = int.Parse(Console.ReadLine());

            //Console.WriteLine("숫자 입력: ");
            //int input = int.Parse(Console.ReadLine());
            //if (input % 2 == 0)
            //{
            //    Console.WriteLine("짝수입니다");
            //}
            //else
            //{
            //    Console.WriteLine("홀수입니다.");
            //}

            //중첩 조건문
            if(DateTime.Now.Hour < 12)
            {
                Console.WriteLine("오전 수업 시간입니다.");
            }
            else
            {
                if(DateTime.Now.Hour <= 18)
                {
                    Console.WriteLine("오후 수업 시간입니다.");
                }
                else
                {
                    Console.WriteLine("수업 종료");
                }

            }
        }
    }
}
