using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            int menu;
            int count;
            int coffee = 2000;
            int latte = 3000;
            int tee = 2500;
            int coffeenum = 0;
            int lattenum = 0;
            int teenum = 0;

            int totnum = 0;
            int totprice = 0;
            int pay = 0;
            totprice = coffee * coffeenum + latte * lattenum + tee * teenum;
            totnum = coffeenum + lattenum + teenum;

            

            while (true)
            {
                Console.WriteLine("\n=====메뉴=====");
                Console.WriteLine("1. 아메리카노 2000원");
                Console.WriteLine("2. 라떼 3000원");
                Console.WriteLine("3. 녹차 2500원");
                Console.WriteLine("99. 주문 종료");
                Console.Write("메뉴 번호 입력: ");

                menu = int.Parse(Console.ReadLine());

                if(menu == 99)
                {
                    break;
                }

                Console.Write("잔 수 입력: ");
                count = int.Parse(Console.ReadLine());

                if (menu == 1)
                {
                    totprice = totprice + (coffee * count);
                    coffeenum = count;
                }
                else if (menu == 2)
                {
                    totprice = totprice + (latte * count);
                    lattenum = count;
                }
                else if (menu ==3)
                {
                    totprice = totprice + (tee * count);
                    teenum = count;
                }
                

                
            }
            
            while (pay < totprice)
            {
                Console.WriteLine("=====영수증=====");
                Console.WriteLine("아메리카노: " + coffeenum + " " + coffeenum * coffee);
                Console.WriteLine("라      떼: " + lattenum + " " + lattenum * latte);
                Console.WriteLine("녹      차: " + teenum + " " + teenum * tee);
                Console.WriteLine("합      계: " + totnum + " " + totprice);
                Console.WriteLine("지불 금액: ");
                pay = int.Parse(Console.ReadLine());
                int change = pay - totprice;
                Console.WriteLine("거스름 돈: "+ change);
                
            }

            

        }
    }
}
