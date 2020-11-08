using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsultingSystemUniversity.Models
{
    public class Feedback
    {
        public int id { get; set; }
        public int account_id { get; set; }
        public string feedback_content { get; set; }
        public string feedback_type { get; set; }
    }
}
