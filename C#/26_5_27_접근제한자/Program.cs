using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _25_5_27_접근제한자
{
    class Program
    {
        class Test
        {
            public void TestMethod()
            {
                Program.Main(new string[] { "" });
            }
        }
        public void TestMethod()
        {
            Program.Main(new string[] { "" });
        }

        static void Main(string[] args)
        {

        }
    }
}
