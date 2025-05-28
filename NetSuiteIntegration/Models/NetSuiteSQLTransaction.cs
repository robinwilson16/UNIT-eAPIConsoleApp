using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NetSuiteIntegration.Models
{
    [Keyless]
    public class NetSuiteSQLTransaction
    {
        public ICollection<NetSuiteLink>? Links { get; set; }
        public int? Count { get; set; }
        public bool? HasMore { get; set; }
        public ICollection<NetSuiteSQLTransactionItem>? Items { get; set; }
        public int? Offset { get; set; }
        public int? TotalResults { get; set; }
    }
}
