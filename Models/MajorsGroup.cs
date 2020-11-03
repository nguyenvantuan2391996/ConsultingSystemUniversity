using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConsultingSystemUniversity.Models
{
    public class MajorsGroup
    {
        [Key]
        public int majors_group_id { get; set; }
        public string majors_group_name { get; set; }
    }
}
