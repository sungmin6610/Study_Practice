using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LM_Stock_Management_System
{
    class Customer:Person
    {
        //public string customer_name { get; set; }
        //public string customer_gender { get; set; }
        //public string customer_Resident_number { get; set; }
        //public string customer_address { get; set; }
        //public string customer_phone { get; set; }
        //public string customer_id { get; set; }
        public int customer_point { get; set; }
        //public string customer_account { get; set; }

        public void Buy_Product(string product_name, int product_count)
        {
            Console.WriteLine($"{id} {name}고객이 {product_name} {product_count}개를 구매했습니다.");
        }
        public void Return_Product(string product_name, int product_count)
        {
            Console.WriteLine($"{id} {name}고객이 {product_name} {product_count}개를 반품했습니다.");
        }
        public void Exchange_Product(string product_id1, string product_name1, string product_id2, string product_name2)
        {
            Console.WriteLine($"{id} {name}고객이 {product_id1} {product_name1}을(를) {product_id2} {product_name2}로 교환했습니다.");
        }
        public void Refunding(string product_name, int product_count)
        {
            Console.WriteLine($"{id} {name}고객이 {product_name} {product_count}개를 환불했습니다.");
        }
    }
}
