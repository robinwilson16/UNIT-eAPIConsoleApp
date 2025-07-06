using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSuiteIntegration.Models
{
    [Keyless]
    public class NetSuiteCustomerPaymentCredit
    {
        public ICollection<NetSuiteLink>? Links { get; set; }
    }
}
