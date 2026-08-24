using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Friday_project
{
    internal class FunctionEx
    {
        
        static string Hello(string name)
        {
            string insa = "제 이름은" + name + "입니다. 만나서 반갑습니다.";

            return insa; 
        }
        static string Sub(int a, int b)
        {
            int result = a - b;
            return $"둣 수의 차은 {result} 입니다";
        }
        static void Main(string[] args)
        {
            Console.WriteLine(Hello("김길동"));
            Console.WriteLine(Sub(1, 2));
            Sub(1, 2);
            Console.WriteLine(Hello("박지수"));

            string result = Hello("이민호");
            Console.WriteLine ($"신입생 인사:{ result}");
            Console.WriteLine(Sub(10, 5));
            Sub(10, 5);
        }
    }
}
