using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace _26_5_6_class3
{// 1. static 클래스: 상속할 수 없고, 인스턴스, 인스턴스 매서드, 생성자 정의할 수 없음.
    static class Utility
    {
        //public int cnt; //static class 에서는 인스턴스 변수 선언 X
        // 2. static 변수(모든 인스턴스가 공유)
        public static int Counter = 0;

        // 3. static 매서드(객체 없이 호출 가능)
        public static void PrintMessage(string msg)
        {
            Console.WriteLine("Utility says: " + msg);
        }
        /*public void MSG() //static class에서는 인스턴스 메서드를 선언할 수 없음.
        {
            Console.WriteLine("aaa");
        }*/
    }
    
    internal class Static
    {
        static void Main(string[] args)
        {
            Utility.Counter++;
            Utility.Counter++;

            Console.WriteLine("Counter값: " + Utility.Counter);
        }

    }
}
