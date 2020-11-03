using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConsultingSystemUniversity.Models
{
    public class Language
    {
        [Key]
        public int language_id { get; set; }
        public string language_name { get; set; }
    }
}
