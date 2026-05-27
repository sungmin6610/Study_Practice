using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace student
{
    class Program
    {
        static void Main(string[] args)
        {
            string haknum = "";
            string name = "";
            string age = "";
            string cellphone = "";
            string addr = "";
            int sub1 = 0;
            int sub2 = 0;
            int sub3 = 0;
            int tot = 0;
            int avg = 0;

            Console.WriteLine("학번을 입력하세요: ");
            haknum = Console.ReadLine();

            Console.WriteLine("이름을 입력하세요: ");
            name = Console.ReadLine();

            Console.WriteLine("나이를 입력하세요: ");
            age = Console.ReadLine();

            Console.WriteLine("휴대전화번호를 입력하세요: ");
            cellphone = Console.ReadLine();

            Console.WriteLine("주소를 입력하세요: ");
            addr = Console.ReadLine();

            Console.WriteLine("과목1 성적을 입력하세요: ");
            sub1 = int.Parse(Console.ReadLine());

            Console.WriteLine("과목2 성적을 입력하세요: ");
            sub2 = int.Parse(Console.ReadLine());

            Console.WriteLine("과목3 성적을 입력하세요: ");
            sub3 = int.Parse(Console.ReadLine());

            tot = sub1 + sub2 + sub3;
            avg = tot / 3;
;
            Console.WriteLine("학번" + haknum);
            Console.WriteLine("이름" + name);
            Console.WriteLine("나이" + age);
            Console.WriteLine("휴대전화번호" + cellphone);
            Console.WriteLine("주소" + addr);
            Console.WriteLine("과목1" + sub1);
            Console.WriteLine("과목2" + sub2);
            Console.WriteLine("과목3" + sub3);
        }
    }
}
