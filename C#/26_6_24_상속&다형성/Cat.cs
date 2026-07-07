using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _26_6_24_상속_다형성
{
    class Cat: Animal
    {
        //public int Age { get; set; }

        //public Cat() { this.Age = 0; }

        //public void Eat() { Console.WriteLine("남남 먹습니다."); }
        //public void Sleep() { Console.WriteLine("쿨쿨 잠을 잡니다."); }
        public void Meow() { Console.WriteLine("냥냥 웁니다."); }
    }
}
