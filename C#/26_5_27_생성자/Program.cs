using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _26_5_27_생성자
{
    class Program
    {
        class Product
        {
            public static int counter = 0;
            public int id;
            public string name;
            public int price;



            public Product(string name, int price)
            {
                Product.counter = counter + 1;
                this.id = counter;
                this.name = name;
                this.price = price;
            }
        }
    
        static void Main(string[] args)
        {
            Product productA = new Product("감자", 2000);
            Product productB = new Product("고구마", 3000);

            Console.WriteLine(productA.id + ":" + productA.name);
            Console.WriteLine(productB.id + ":" + productB.name);
            Console.WriteLine(Product.counter + "개 생성되었습니다.");
        }
    }
}
