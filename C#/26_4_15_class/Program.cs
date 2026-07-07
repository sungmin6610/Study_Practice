using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _26._4._15_class
{
    class Program
    {
        static void Main(string[] args)
        {
            //Random 클래스를 사용한 임의의 정수 생성
            //Random random = new Random();
            //Console.WriteLine(random.Next(10, 100));
            //Console.WriteLine(random.Next(10, 100));
            //Console.WriteLine(random.Next(10, 100));
            //Console.WriteLine(random.Next(10, 100));

            //Random 클래스를 사용한 임의의 실수 생성
            //Console.WriteLine(random.NextDouble());
            //Console.WriteLine(random.NextDouble());
            //Console.WriteLine(random.NextDouble());
            //Console.WriteLine(random.NextDouble());
           
            //Math Class
            //Console.WriteLine(Math.Abs(-52273));
            //Console.WriteLine(Math.Ceiling(52.273));
            //Console.WriteLine(Math.Floor(52.273));
            //Console.WriteLine(Math.Max(52,273));
            //Console.WriteLine(Math.Min(52,273));
            //Console.WriteLine(Math.Round(52.273));

            //List class
            List<int> list = new List<int>() { 52, 273, 32, 64 }; //인스턴스 생성

            //리스트에 요소 추가
            //list.Add(52);
            //list.Add(273);
            //list.Add(32);
            //list.Add(64);

            //반복수행
            foreach (var item in list)
            {
                Console.WriteLine("Count: " + list.Count + "\titem: " + item);
            }

            list.Remove(273);
            foreach(var item in list)
            {
                Console.WriteLine("Count: " + list.Count + "\titem: " + item);
            }

            list.RemoveAll(n => n > 50);
            foreach (var item in list)
            {
                Console.WriteLine("Count: " + list.Count + "\titem: " + item);
            }

            list.Clear();
            if (list.Count != 0)
            {
                foreach (var item in list)
                {
                    Console.WriteLine("Count: " + list.Count + "\titem: " + item);
                }
            }
            else
            {
                Console.WriteLine("list 객체에 데이터가 없습니다.");
            }

            //Math class
            Console.WriteLine(Math.Abs(-52273));
            Console.WriteLine(Math.Ceiling(52.273));
            Console.WriteLine(Math.Floor(52.273));
            Console.WriteLine(Math.Max(52,273));
            Console.WriteLine(Math.Min(52,273));
            Console.WriteLine(Math.Round(52.273));
        }
    }
}
