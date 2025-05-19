using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NetSuiteIntegration.Models
{
    [Keyless]
    public class NetSuiteAddress
    {
        public ICollection<NetSuiteLink>? Links { get; set; }
        public string? Addr1 { get; set; }
        public string? Addr2 { get; set; }
        public string? Addressee { get; set; }
        public string? AddrText { get; set; }
        public string? City { get; set; }
        public NetSuiteAddressCountry? Country { get; set; }
        public bool? Override { get; set; }
        public string? Zip { get; set; }
    }
}
