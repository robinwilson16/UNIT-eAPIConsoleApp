using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NetSuiteIntegration.Models
{
    [Keyless]
    public class NetSuiteInvoiceAccount
    {
        public ICollection<NetSuiteLink>? Links { get; set; }
        public string? ID { get; set; }
        public string? RefName { get; set; }
    }
}
