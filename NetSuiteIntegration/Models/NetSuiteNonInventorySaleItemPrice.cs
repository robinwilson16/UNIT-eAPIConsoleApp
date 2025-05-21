using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NetSuiteIntegration.Models
{
    [Keyless]
    public class NetSuiteNonInventorySaleItemPrice
    {
        public List<NetSuiteLink>? Links { get; set; }
    }
}
