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
        public string? Address { get; set; }
        public string? Address1
        {
            get
            {
                if (Address != null)
                {
                    string[] addressParts = Address.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 0 ? addressParts[0] : null;
                }
                return null;
            }
        }
        public string? Address2
        {
            get
            {
                if (Address != null)
                {
                    string[] addressParts = Address.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 1 ? addressParts[1] : null;
                }
                return null;
            }
        }
        public string? Address3
        {
            get
            {
                if (Address != null)
                {
                    string[] addressParts = Address.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 2 ? addressParts[2] : null;
                }
                return null;
            }
        }
        public string? Address4
        {
            get
            {
                if (Address != null)
                {
                    string[] addressParts = Address.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 3 ? addressParts[3] : null;
                }
                return null;
            }
        }
        public string? Address5
        {
            get
            {
                if (Address != null)
                {
                    string[] addressParts = Address.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    return addressParts.Length > 4 ? addressParts[4] : null;
                }
                return null;
            }
        }
        [MaxLength(8)]
        public string? PostCode { get; set; }
        public string? EmailAddress { get; set; }
        public string? Mobile { get; set; }
        public string? HomePhone { get; set; }
        public string? AcademicYearCode { get; set; }
        public string? AcademicYearName { get; set; }
        
        [Column(TypeName = "decimal(19,4)")]
        [DataType(DataType.Currency)]
        public decimal? FeeNet { get; set; }
        [Column(TypeName = "decimal(19,4)")]
        [DataType(DataType.Currency)]
        public decimal? FeeGross { get; set; }
    }
}
