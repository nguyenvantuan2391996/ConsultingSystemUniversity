using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConsultingSystemUniversity.Models
{
    public class SenderReceive
    {
        [Key]
        public int id { get; set; }
        public int message_id { get; set; }
        public int sender_id { get; set; }
        public int receive_id { get; set; }
    }
}
