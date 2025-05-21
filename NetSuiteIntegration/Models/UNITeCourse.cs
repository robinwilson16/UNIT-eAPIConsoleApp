using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetSuiteIntegration.Models
{
    public class UNITeCourse
    {
        [Key]
        [Column(TypeName = "decimal(16,0)")]
        public decimal CourseID { get; set; }
        public string? AcademicYearCode { get; set; }
        public string? AcademicYearName { get; set; }
        public string? CampusCode { get; set; }
        public string? CampusName { get; set; }
        public string? DepartmentCode { get; set; }
        public string? DepartmentName { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseTitle { get; set; }
        public string? CourseTypeCode { get; set; }
        public string? CourseTypeName { get; set; }
        public string? SubjectCode { get; set; }
        public string? SubjectName { get; set; }
        public string? LevelCode { get; set; }
        public string? LevelName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Column(TypeName = "decimal(19,4)")]
        [DataType(DataType.Currency)]
        public decimal? Fee { get; set; }
        [Column(TypeName = "decimal(19,4)")]
        [DataType(DataType.Currency)]
        public decimal? Deposit { get; set; }
    }
}
