using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NetSuiteIntegration.Models
{
    [Keyless]
    public class NetSuiteInvoiceItemItem
    {
        public ICollection<NetSuiteLink>? Links { get; set; }
        public double? Amount { get; set; }
        public NetSuiteInvoiceItemItemClass? Class { get; set; }
        public string? CustItem1 { get; set; }
        public string? CustItem2 { get; set; }
        public NetSuiteInvoiceItemItemItem? Item { get; set; }
        public NetSuiteInvoiceItemItemLocation? Location { get; set; }
        public NetSuiteInvoiceItemItemSubsidiary? Subsidiary { get; set; }
        public NetSuiteInvoiceItemItemTerms? Terms { get; set; }

    }
}
