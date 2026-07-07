using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _26_6_24_상속_다형성
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Animal> Animals = new List<Animal>()
            {
                new Dog(), new Cat(), new Cat(), new Dog(),
                new Dog(), new Cat(), new Dog(), new Cat()
            };
            

            foreach(var item in Animals)
            {
                item.Eat();
                item.Sleep();
                ((Dog)item).Bark();
                ((Cat)item).Meow();
            }
        }
    }
}
