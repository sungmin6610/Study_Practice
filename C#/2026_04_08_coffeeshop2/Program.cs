using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2026_04_08_coffeeshop2
{
    class Program
    {
        static void Main(string[] args)
        {
            int coffee = 2000;
            int latte = 3000;
            int tee = 2500;
            int coffeenum = 0;
            int lattenum = 0;
            int teenum = 0;

            int totnum = 0;
            int totprice = 0;
            
            
            Console.WriteLine("\n메뉴");
            Console.WriteLine("1. 아메리카노 2000원");
            Console.WriteLine("2. 라떼 3000원");
            Console.WriteLine("3. 녹차 2500원");
            Console.WriteLine("99. 주문 종료");
            
            while (true)
            {
                Console.Write("\n메뉴선택: ");

                if 
            }

            Console.WriteLine("아메리카노 개수: ");
            coffeenum = int.Parse(Console.ReadLine());
            Console.WriteLine("라떼 개수: ");
            lattenum = int.Parse(Console.ReadLine());
            Console.WriteLine("녹차 개수: ");
            teenum = int.Parse(Console.ReadLine());


            totprice = coffee * coffeenum + latte * lattenum + tee * teenum;
            totnum = coffeenum + lattenum + teenum;


            Console.WriteLine("영수증");
            Console.WriteLine("아메리카노 개수: " + coffeenum + " " + coffeenum * coffee);
            Console.WriteLine("라떼 개수: " + lattenum + " " + lattenum * latte);
            Console.WriteLine("녹차 개수: " + teenum + " " + teenum * tee);
            Console.WriteLine("합계: " + totnum + " " + totprice);
            
        }
    }
}
