using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            int sum = 0;                 // 결과값 누적을 위한 변수선언
            for (int  i= 1; i <= 100; i++) // 1부터 100까지 반복
            {
                sum += i; // sum = sum + 1과 같음, 누적합을 구하는 부분
            }

            Console.WriteLine("1부터 100까지의 합: " + sum);  // 결과값 출력

            /* 새로운 챕터
             * Console.WriteLine 매서드와
             * Console.Write 매서드 활용법 예제*/
              Console.WriteLine("Write");
              Console.Write("Write");
              Console.Write("Write");
              Console.Write("Write");
              Console.WriteLine("Write");
              Console.WriteLine("Write");
              Console.WriteLine("Write");
              
        }
    }
}