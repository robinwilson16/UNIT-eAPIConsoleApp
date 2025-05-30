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
        public DateTime? AcademicYearStartDate { get; set; }
        public DateTime? AcademicYearEndDate { get; set; }
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
        public DateTime? StartDateCourse { get; set; }
        public DateTime? EndDateCourse { get; set; }
        public DateTime? StartDateProgramme { get; set; }
        public DateTime? EndDateProgramme { get; set; }
        [Column(TypeName = "decimal(19,4)")]
        [DataType(DataType.Currency)]
        public decimal? Fee { get; set; }
        [Column(TypeName = "decimal(19,4)")]
        [DataType(DataType.Currency)]
        public decimal? Deposit { get; set; }

        public string? CourseCodeNextYear
        {
            get
            {
                string[]? courseCodeParts = CourseCode?.Split("/");
                if (courseCodeParts?.Count() == 5)
                {
                    if (EndDateProgramme <= EndDateCourse || courseCodeParts[3] == "Y3")
                    {
                        return null; // If programme ends this year or is already in the final year (Y3), do not advance the course code
                    }
                    else
                    {
                        //Advance course year by 1
                        Int32.TryParse(courseCodeParts[4], out int courseYear);
                        string courseYearString = (courseYear + 1).ToString();

                        //Advance year number
                        Int32.TryParse(courseCodeParts[3].Replace("Y", ""), out int yearNumber);
                        string yerNumberString = (yearNumber + 1).ToString();

                        return $"{courseCodeParts[0]}/{courseCodeParts[1]}/{courseCodeParts[2]}/Y{yerNumberString}/{courseYear}";
                    }
                }
                
                return null;
            }
        }
        public DateTime? StartDateNextYear
        {
            get
            {
                string[]? courseCodeParts = CourseCode?.Split("/");
                if (courseCodeParts?.Count() == 5)
                {
                    if (EndDateProgramme <= EndDateCourse || courseCodeParts[3] == "Y3")
                    {
                        return null; // If programme ends this year or is already in the final year (Y3), do not advance the course code
                    }
                    else
                    {
                        if (StartDateCourse == null)
                            return null;

                        var nextYear = StartDateCourse.Value.Year + 1;
                        var month = StartDateCourse.Value.Month;

                        // Last day of the same month next year
                        var lastDay = new DateTime(nextYear, month, DateTime.DaysInMonth(nextYear, month));

                        // Calculate offset: Monday = 1, so (DayOfWeek + 6) % 7 gives days since last Monday
                        int offset = ((int)lastDay.DayOfWeek + 6) % 7;
                        var lastMonday = lastDay.AddDays(-offset);

                        return lastMonday;
                    }
                }

                return null;
            }
        }

        public DateTime? EndDateNextYear
        {
            get
            {
                string[]? courseCodeParts = CourseCode?.Split("/");
                if (courseCodeParts?.Count() == 5)
                {
                    if (EndDateProgramme <= EndDateCourse || courseCodeParts[3] == "Y3")
                    {
                        return null; // If programme ends this year or is already in the final year (Y3), do not advance the course code
                    }
                    else
                    {
                        if (StartDateCourse == null)
                            return null;

                        var nextYear = StartDateCourse.Value.Year + 1;
                        var month = StartDateCourse.Value.Month;

                        // Last day of the same month next year
                        var lastDay = new DateTime(nextYear, month, DateTime.DaysInMonth(nextYear, month));

                        // Calculate offset: Friday = 5, so (DayOfWeek - 5 + 7) % 7 gives days since last Friday
                        int offset = ((int)lastDay.DayOfWeek - (int)DayOfWeek.Friday + 7) % 7;
                        var lastFriday = lastDay.AddDays(-offset);

                        return lastFriday;
                    }
                }

                return null;
            }
        }

        //Store NetSuite Sale Item ID once found for linking
        public string? NetSuiteNonInventorySaleItemID { get; set; }
    }
}
