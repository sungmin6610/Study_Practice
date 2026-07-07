using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _26._4._15.@continue
{
    class Program
    {
        static void Main(string[] args)
        {   //대문자화, 소문자화
            //string input = "Potato Tomato";
            //Console.WriteLine(input.ToUpper());
            //Console.WriteLine(input.ToLower());

            //문자열 자르기
            //string input = "감자 고구마 토마토";
            //string[] inputs = input.Split(new char[] { ' ' });

            //foreach (var item in inputs)
            //{
            //    Console.WriteLine(item);
            //}
            ////foreach문을 for문으로 바꾸면
            //for(int i = 0; i < 3; i++)
            //{
            //    Console.WriteLine(inputs[i]);
            //}

            //문자열 양옆의 공백 제거
            string input = "      test      ";
            Console.WriteLine("::" + input.Trim() + "::" );
            Console.WriteLine("::" + input.TrimStart() + "::");
            Console.WriteLine("::" + input.TrimEnd() + "::");

            //Thread.Sleep 메서드
            Thread.Sleep(10000);
            Console.Clear();
            Console.SetCursorPosition(5, 5);
            Console.Write("메서드 호출 후");

        }
    }
}
