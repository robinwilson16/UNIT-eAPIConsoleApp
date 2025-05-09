using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSuiteIntegration.Models
{
    public class NetSuiteCustomerReceivablesAccount
    {
        public List<NetSuiteCustomerLink>? Links { get; set; }
        public string? ID { get; set; }
        public string? RefName { get; set; }
    }
}
