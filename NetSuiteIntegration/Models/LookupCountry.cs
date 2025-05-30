using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSuiteIntegration.Models
{
    public class LookupCountry
    {
        [Key]
        [MaxLength(50)]
        public string? UNITeCountryCode { get; set; }
        [MaxLength(250)]
        public string? NetSuiteCountryName { get; set; }
    }
}
