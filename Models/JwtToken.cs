using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsultingSystemUniversity.Models
{
    public class JwtToken
    {
        public int id { get; set; }
        public string refresh_token { get; set; }
        public string token { get; set; }
        public DateTime expiry { get; set; }
        public string role { get; set; }
    }
}
