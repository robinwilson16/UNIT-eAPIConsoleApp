using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSuiteIntegration.Models
{
    public class LookupCampus
    {
        [Key]
        [MaxLength(100)]
        public string? UNITeCampusCode { get; set; }
        [MaxLength(100)]
        public string? NetSuiteLocationID { get; set; }
        [MaxLength(100)]
        public string? NetSuiteLocationName { get; set; }
        [MaxLength(100)]
        public string? NetSuiteSubsiduaryID { get; set; }
        [MaxLength(100)]
        public string? NetSuiteFacultyID { get; set; }
    }
}
