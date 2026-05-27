using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM_Stock_Management_System
{
    class Employee
    {
        public string employee_id { get; set; }
        public string employee_name { get; set; }
        public string employee_Resident_number { get; set; }
        public string employee_gender { get; set; }
        public string employee_address { get; set; }
        public string employee_phone { get; set; }
        public string employee_rank { get; set; }
        public string employee_birth { get; set; }
        public string employee_account { get; set; }
        public string employee_job { get; set; }
        public string employee_status { get; set; }

        public void Go_To_Work()
        {
            employee_status = "출근";
            Console.WriteLine($"id-{employee_id}: {employee_name}직원이 {employee_status}하였습니다.");
        }

        public void Stock_Management()
        {
            employee_job = "재고관리";
            Console.WriteLine($"id-{employee_id}: {employee_name}직원이 {employee_job}하고 있습니다.");
        }

        public void Check_Out()
        {
            employee_job = "계산";
            Console.WriteLine($"id-{employee_id}: {employee_name}직원이 {employee_job}하고 있습니다.");
        }

        public void Cleaning()
        {
            employee_job = "청소";
            Console.WriteLine($"id-{employee_id}: {employee_name}직원이 {employee_job}하고 있습니다.");
        }

        public void Go_To_Home()
        {
            employee_status = "퇴근";
            Console.WriteLine($"id-{employee_id}: {employee_name}직원이 {employee_status}하였습니다.");
        }
    }
}
