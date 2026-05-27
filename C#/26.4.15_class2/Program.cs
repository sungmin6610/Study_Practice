using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _26._4._15_class2
{
    class Program
    {
        class Product
        {
            public string name;
            public int price;
        }
        static void Main(string[] args)
        {
            Product product = new Product();


            //product.name = "감자";
            //product.price = 2000;
            Product product1 = new Product() { name = "감자", price = 2000 };
            Product product2 = new Product() { name = "고구마", price = 3000 };

            Console.WriteLine(product1.name + " : " + product.price + "원");
            Console.WriteLine(product2.name + " : " + product.price + "원");
        }
    }

    
}
