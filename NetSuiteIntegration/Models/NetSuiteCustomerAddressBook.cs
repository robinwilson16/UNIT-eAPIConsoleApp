using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NetSuiteIntegration.Models
{
    [Keyless]
    public class NetSuiteCustomerAddressBook
    {
        public ICollection<NetSuiteLink>? Links { get; set; }
        public ICollection<NetSuiteAddressBook>? Items { get; set; }
    }
}
