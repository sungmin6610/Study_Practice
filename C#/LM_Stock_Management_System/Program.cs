using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM_Stock_Management_System
{
    class Program
    {
        static void Main(string[] args)
        {
            Employee employee1 = new Employee
            {
                id = "001",
                name = "송성민",
                phone = "010-9999-9999",
                employee_rank = "사원",
                employee_birth = "01-03-13",
                gender = "남성",
                Resident_number = "010101-010101",
                account = "110-481-097869",
                address = "경기 화성시 동탄구",
                employee_job = "없음",
                employee_status = "퇴근"
            };

            employee1.Go_To_Work();
            employee1.Stock_Management();
            employee1.Check_Out();
            employee1.Cleaning();
            employee1.Go_To_Home();

            Customer customer1 = new Customer
            {
                id = "C-001",
                name = "서준원",
                phone = "010-1111-1111",
                account = "123-45-67890123",
                address = "경기 화성시 동탄구",
                gender = "남성",
                customer_point = 20,
                Resident_number = "000202-020202"
            };

            customer1.Buy_Product("홈런볼", 10);
            customer1.Return_Product("홈런볼", 10);
            customer1.Exchange_Product("P-001", "진라면", "P-002", "홈런볼");
            customer1.Refunding("진라면", 5);

            Partner_Company partner = new Partner_Company
            {
                company_name = "오뚜기",
                company_president = "김동영",
                company_phone = "02-3333-3333",
                company_account = "987-65-43210987",
                company_product_name = "진라면",
                company_product_count = 1000
            };

            partner.Production("P-001");
            partner.Order_Receive("P-001");
            partner.Delivering("P-001");
            partner.Collect_Money("오뚜기", "987-65-43210987");

            Product product1 = new Product
            {
                product_id = "P-001",
                product_category = "식품",
                product_name = "진라면",
                product_count = 1000,
                product_price = 5000
            };

            Product product2 = new Product
            {
                product_id = "P-002",
                product_category = "식품",
                product_name = "홈런볼",
                product_count = 500,
                product_price = 1500
            };
        }
    }
}
