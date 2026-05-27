using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM_Stock_Management_System
{
    class Product
    {
        public string product_id { get; set; }
        public string product_name { get; set; }
        public int product_count { get; set; }
        public int product_price { get; set; }
        public string product_expiration_date { get; set; }
        public string product_category { get; set; }
        
    }
}
