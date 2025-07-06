using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSuiteIntegration.Models
{
    [Keyless]
    public class NetSuiteCustomerPaymentUndepFunds
    {
        public bool? ID { get; set; }
        public string? RefName { get; set; }
    }
}
