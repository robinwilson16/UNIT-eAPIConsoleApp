using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace NetSuiteIntegration.Models
{
    public class UNITeStudent
    {
        [Key]
        [Column(TypeName = "decimal(16,0)")]
        public decimal? StudentID { get; set; }
        public string? StudentRef { get; set; }
        public string? ExternalRef { get; set; }
        public string? ERPID { get; set; }
        public string? Surname { get; set; }
        public string? Forename { get; set; }
        public string? PreferredName { get; set; }
        public string? TitleCode { get; set; }
        public string? TitleName { get; set; }
        public string? GenderCode { get; set; }
        public string? GenderName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? UCASPersonalID { get; set; }

        [MaxLength(10)]
        public string? ULN { get; set; }

        [JsonIgnore]
        public int? uLN
        {
            get
            {
                int.TryParse(ULN, out int ulnInt);
                return ULN == null ? null : ulnInt;
            }
        }
        [Column(TypeName = "decimal(16,0)")]
        public decimal? AddressIDMain { get; set; }
        public string? AddressMain { get; set; }
        public string? AddressMainEncoded
        {
            get
            {
                if (AddressMain != null)
                {
                    string addressEncoded = AddressMain.Replace(Environment.NewLine, "\n");
                    return addressEncoded;
                }
                return null;
            }
        }
        public string? Address1Main
        {
            get
            {
                if (AddressMain != null)
                {
                    string[] addressParts = AddressMain.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 0 ? addressParts[0] : null;
                }
                return null;
            }
        }
        public string? Address2Main
        {
            get
            {
                if (AddressMain != null)
                {
                    string[] addressParts = AddressMain.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 1 ? addressParts[1] : null;
                }
                return null;
            }
        }
        public string? Address3Main
        {
            get
            {
                if (AddressMain != null)
                {
                    string[] addressParts = AddressMain.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 2 ? addressParts[2] : null;
                }
                return null;
            }
        }
        public string? Address4Main
        {
            get
            {
                if (AddressMain != null)
                {
                    string[] addressParts = AddressMain.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 3 ? addressParts[3] : null;
                }
                return null;
            }
        }
        public string? Address5Main
        {
            get
            {
                if (AddressMain != null)
                {
                    string[] addressParts = AddressMain.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 4 ? addressParts[4] : null;
                }
                return null;
            }
        }
        public string? CountryCodeMain { get; set; }
        public string? CountryNameMain { get; set; }
        public string? AddressMainType { get; set; }
        [MaxLength(8)]
        public string? PostCodeMain { get; set; }
        [Column(TypeName = "decimal(16,0)")]
        public decimal? AddressIDTermTime { get; set; }
        public string? AddressTermTime { get; set; }
        public string? AddressTermTimeEncoded
        {
            get
            {
                if (AddressTermTime != null)
                {
                    string addressEncoded = AddressTermTime.Replace(Environment.NewLine, "\n");
                    return addressEncoded;
                }
                return null;
            }
        }
        public string? Address1TermTime
        {
            get
            {
                if (AddressTermTime != null)
                {
                    string[] addressParts = AddressTermTime.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 0 ? addressParts[0] : null;
                }
                return null;
            }
        }
        public string? Address2TermTime
        {
            get
            {
                if (AddressTermTime != null)
                {
                    string[] addressParts = AddressTermTime.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 1 ? addressParts[1] : null;
                }
                return null;
            }
        }
        public string? Address3TermTime
        {
            get
            {
                if (AddressTermTime != null)
                {
                    string[] addressParts = AddressTermTime.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 2 ? addressParts[2] : null;
                }
                return null;
            }
        }
        public string? Address4TermTime
        {
            get
            {
                if (AddressTermTime != null)
                {
                    string[] addressParts = AddressTermTime.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 3 ? addressParts[3] : null;
                }
                return null;
            }
        }
        public string? Address5TermTime
        {
            get
            {
                if (AddressTermTime != null)
                {
                    string[] addressParts = AddressTermTime.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 4 ? addressParts[4] : null;
                }
                return null;
            }
        }
        public string? CountryCodeTermTime { get; set; }
        public string? CountryNameTermTime { get; set; }
        [MaxLength(8)]
        public string? PostCodeTermTime { get; set; }
        [Column(TypeName = "decimal(16,0)")]
        public decimal? AddressIDHome { get; set; }
        public string? AddressHome { get; set; }
        public string? AddressHomeEncoded
        {
            get
            {
                if (AddressHome != null)
                {
                    string addressEncoded = AddressHome.Replace(Environment.NewLine, "\n");
                    return addressEncoded;
                }
                return null;
            }
        }
        public string? Address1Home
        {
            get
            {
                if (AddressHome != null)
                {
                    string[] addressParts = AddressHome.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 0 ? addressParts[0] : null;
                }
                return null;
            }
        }
        public string? Address2Home
        {
            get
            {
                if (AddressHome != null)
                {
                    string[] addressParts = AddressHome.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 1 ? addressParts[1] : null;
                }
                return null;
            }
        }
        public string? Address3Home
        {
            get
            {
                if (AddressHome != null)
                {
                    string[] addressParts = AddressHome.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 2 ? addressParts[2] : null;
                }
                return null;
            }
        }
        public string? Address4Home
        {
            get
            {
                if (AddressHome != null)
                {
                    string[] addressParts = AddressHome.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 3 ? addressParts[3] : null;
                }
                return null;
            }
        }
        public string? Address5Home
        {
            get
            {
                if (AddressHome != null)
                {
                    string[] addressParts = AddressHome.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 4 ? addressParts[4] : null;
                }
                return null;
            }
        }
        public string? CountryCodeHome { get; set; }
        public string? CountryNameHome { get; set; }
        [MaxLength(8)]
        public string? PostCodeHome { get; set; }
        [Column(TypeName = "decimal(16,0)")]
        public decimal? AddressIDInvoice { get; set; }
        public string? AddressInvoice { get; set; }
        public string? AddressInvoiceEncoded
        {
            get
            {
                if (AddressInvoice != null)
                {
                    string addressEncoded = AddressInvoice.Replace(Environment.NewLine, "\n");
                    return addressEncoded;
                }
                return null;
            }
        }
        public string? Address1Invoice
        {
            get
            {
                if (AddressInvoice != null)
                {
                    string[] addressParts = AddressInvoice.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 0 ? addressParts[0] : null;
                }
                return null;
            }
        }
        public string? Address2Invoice
        {
            get
            {
                if (AddressInvoice != null)
                {
                    string[] addressParts = AddressInvoice.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 1 ? addressParts[1] : null;
                }
                return null;
            }
        }
        public string? Address3Invoice
        {
            get
            {
                if (AddressInvoice != null)
                {
                    string[] addressParts = AddressInvoice.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 2 ? addressParts[2] : null;
                }
                return null;
            }
        }
        public string? Address4Invoice
        {
            get
            {
                if (AddressInvoice != null)
                {
                    string[] addressParts = AddressInvoice.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 3 ? addressParts[3] : null;
                }
                return null;
            }
        }
        public string? Address5Invoice
        {
            get
            {
                if (AddressInvoice != null)
                {
                    string[] addressParts = AddressInvoice.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 4 ? addressParts[4] : null;
                }
                return null;
            }
        }
        public string? CountryCodeInvoice { get; set; }
        public string? CountryNameInvoice { get; set; }
        [MaxLength(8)]
        public string? PostCodeInvoice { get; set; }
        public string? EmailAddress { get; set; }
        public string? Mobile { get; set; }
        public string? HomePhone { get; set; }
        public string? AcademicYearCode { get; set; }
        public string? AcademicYearName { get; set; }
        public DateTime? AcademicYearStartDate { get; set; }
        public DateTime? AcademicYearEndDate { get; set; }
        public decimal? CourseID { get; set; }
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
        public string? EnrolmentType { get; set; }
        public DateTime? StartDateEnrol { get; set; }
        public DateTime? ExpectedEndDateEnrol { get; set; }
        public DateTime? ActualEndDateEnrol { get; set; }
        public DateTime? StartDateProgramme { get; set; }
        public DateTime? ExpectedEndDateProgramme { get; set; }
        public DateTime? ActualEndDateProgramme { get; set; }
        public DateTime? StartDateCourse { get; set; }
        public DateTime? EndDateCourse { get; set; }
        public string? EnrolmentStatusCode { get; set; }
        public string? EnrolmentStatusName { get; set; }
        public string? ProgressionCode { get; set; }
        public string? ProgressionName { get; set; }
        public string? ProgressionCourseCode { get; set; }
        [Column(TypeName = "decimal(19,4)")]
        [DataType(DataType.Currency)]
        public decimal? FeeNet { get; set; }
        [Column(TypeName = "decimal(19,4)")]
        [DataType(DataType.Currency)]
        public decimal? FeeGross { get; set; }
        [Column(TypeName = "decimal(19,4)")]
        [DataType(DataType.Currency)]
        public decimal? FeeOriginal { get; set; }
        [Column(TypeName = "decimal(19,4)")]
        [DataType(DataType.Currency)]
        public decimal? FeeDiscount { get; set; }

        [JsonIgnore]
        public string? CampusFromCourseCode
        {
            get
            {
                string[]? courseCodeParts = CourseCode?.Split("/");
                if (courseCodeParts?.Count() == 5)
                {
                    return courseCodeParts[0];
                }

                return null;
            }
        }

        //Store NetSuite Customer ID once found for linking
        [JsonIgnore]
        public string? NetSuiteCustomerID { get; set; }

        //From Lookup Tables Populated Later
        [JsonIgnore]
        public string? NetSuiteLocationID { get; set; }
        [JsonIgnore]
        public string? NetSuiteLocationName { get; set; }
        [JsonIgnore]
        public string? NetSuiteSubsiduaryID { get; set; }
        [JsonIgnore]
        public string? NetSuiteFacultyID { get; set; }
        [JsonIgnore]
        public string? NetSuiteCountryNameMain { get; set; }
        [JsonIgnore]
        public string? NetSuiteCountryNameTermTime { get; set; }
        [JsonIgnore]
        public string? NetSuiteCountryNameHome { get; set; }
        [JsonIgnore]
        public string? NetSuiteCountryNameInvoice { get; set; }
    }
}
