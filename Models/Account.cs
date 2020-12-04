using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConsultingSystemUniversity.Models
{
    public class Account
    {
        public int id { get; set; }
        public int language_id { get; set; }
        public string account_name { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string role { get; set; }
        public string status { get; set; }

        public string image_url { get; set; }
    }
}
