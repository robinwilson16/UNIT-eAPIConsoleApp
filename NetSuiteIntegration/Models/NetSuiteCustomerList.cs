using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NetSuiteIntegration.Models
{
    [Keyless]
    public class NetSuiteCustomerList
    {
        public ICollection<NetSuiteCustomerListLink>? Links { get; set; }
        public int? Count { get; set; }
        public bool? HasMore { get; set; }
        public ICollection<NetSuiteCustomerListItem>? Items { get; set; }
        public int? Offset { get; set; }
        public int? TotalResults { get; set; }
    }
}
