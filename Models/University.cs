using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConsultingSystemUniversity.Models
{
    public class University
    {
        [Key]
        public int university_code { get; set; }
        public string university_name { get; set; }
        public double rank { get; set; }
        public string location { get; set; }
        public string link_website { get; set; }
    }
}
