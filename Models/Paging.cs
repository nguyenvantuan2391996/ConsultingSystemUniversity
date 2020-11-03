using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsultingSystemUniversity.Models
{
    public class Paging
    {
        public int limit { get; set; }
        public int offset { get; set; }
        public Account accounts { get; set; }
    }
}
