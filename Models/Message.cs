using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConsultingSystemUniversity.Models
{
    public class Message
    {
        [Key]
        public int message_id { get; set; }
        public string message_content { get; set; }
        public DateTime message_time { get; set; }
        public string is_read { get; set; }
    }
}
