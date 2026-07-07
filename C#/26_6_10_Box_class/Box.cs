using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _26_6_10_Box_class
{
    internal class Box
    {
        public int width;
        public int height;


        public Box(int width, int height)
        {
            if(width > 0 || height > 0)
            {
                this.width = width;
                this.height = height;
            }
            else
            {
                Console.WriteLine("너비와 높이는 자연수로 초기화해주세요!");
            }

        }

        public int Area()
        {
            return this.width * this.height;
        }

        public int GetWidth()
        {
            return this.width;
        }

        public int GetHeight()
        {
            return this.height;
        }

        public void setWidth()
        {
            if(width > 0) 
            {
                this.width = width;
            }
            else
            {
                Console.WriteLine("너비는 자연수를 입력해주세요.");
            }
        }

        public void setHeight()
        {
            if(height > 0)
            {
                this.height = height;
            }
            else
            {
                Console.WriteLine("높이는 자연수를 입력해주세요.");
            }
        }
    }
}
