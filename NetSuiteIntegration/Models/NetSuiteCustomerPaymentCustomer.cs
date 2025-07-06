using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSuiteIntegration.Models
{
    [Keyless]
    public class NetSuiteCustomerPaymentCustomer
    {
        //public ICollection<NetSuiteLink>? Links { get; set; }
        public string? ID { get; set; }
        //public string? RefName { get; set; }
    }
}
