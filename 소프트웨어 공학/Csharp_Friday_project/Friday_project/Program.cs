using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friday_project
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("숫자를 입력하세요: ");
            int number = int.Parse(Console.ReadLine());
            int sum = 0;

            for (int i = 1; ; i = i + 2)//while(true)
            {
                if (i > number)
                    break;
                sum += i;

            }

            Console.WriteLine($"1부터 {number}까지의 홀수의 합은 {sum}입니다.");

            int number = Console.WriteLine("숫자를 입력하세요");
            int number = int.Parse(Console.ReadLine());
            //int number1 = Convert.Tolnt32(Console.ReadLine());

            if (number > 0)
                // Console.WriteLine(number + "는(은) 양수입니다.");
                Console.WriteLine($"{number}는(은) 양수입니다.");

            else if (number < 0)
                Console.WriteLine($"{number}는 음수입니다.");

            else
                Console.WriteLine($"입력된 수는 {number}입니다.");

        }
    }
}
