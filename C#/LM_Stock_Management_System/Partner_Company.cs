using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace LM_Stock_Management_System
{
    class Partner_Company
    {
        public string company_name { get; set; }
        public string company_phone { get; set; }
        public string company_president { get; set; }
        public string business_registration_number { get; set; }
        public string company_product_name { get; set; }
        public string company_account { get; set; }
        public int company_product_count { get; set; }

        public void Production(string product_id)
        {
            Console.WriteLine($"{product_id} {company_product_name}상품을(를) {company_product_count}개 생산 합니다.");
        }

        public void Order_Receive(string product_id)
        {
            Console.WriteLine($"{product_id} {company_product_name}상품을(를) {company_product_count}개 주문 합니다.");
        }

        public void Delivering(string product_id)
        {
            Console.WriteLine($"{product_id} {company_product_name}상품을(를) {company_product_count}개 납품중 입니다.");
        }

        public void Collect_Money(string company_name, string company_account)
        {
            Console.WriteLine($"{company_name} {company_account}계좌로 입금합니다.");
        }
    }
}
