using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSuiteIntegration.Models
{
    public class UNITeRepGen
    {
        [Key]
        public UNITeRepGenType? Type { get; set; }
        public string? Reference { get; set; }
        public string? Name { get; set; }
    }

    public enum UNITeRepGenType
    {
        Enrolment,
        Course,
        Fee,
        Refund
    }
}
