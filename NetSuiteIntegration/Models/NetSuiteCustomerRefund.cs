using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static NetSuiteIntegration.Models.SharedEnum;

namespace NetSuiteIntegration.Models
{
    public class NetSuiteCustomerRefund
    {
        public ICollection<NetSuiteLink>? Links { get; set; }
        public NetSuiteCustomerRefundAccount? Account { get; set; }
        public string? Address { get; set; }
        public NetSuiteCustomerRefundApply? Apply { get; set; }
        public NetSuiteCustomerRefundAracct? Aracct { get; set; }
        public double? Balance { get; set; }
        public bool? Cleared { get; set; }
        public string? ClearedDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public NetSuiteCustomerRefundCurrency? Currency { get; set; }
        public bool? Custbody9997AutocashAssertionField { get; set; }
        public bool? Custbody9997IsForEpDd { get; set; }
        public NetSuiteCustomerRefundCustbodyAtlasNoHdn? CustbodyAtlasNoHdn { get; set; }
        public NetSuiteCustomerRefundCustbodyAtlasYesHdn? CustbodyAtlasYesHdn { get; set; }
        public NetSuiteCustomerRefundCustbodyAznCurrentUser? CustbodyAznCurrentUser { get; set; }
        public NetSuiteCustomerRefundCustbodyNondeductibleRefTran? CustbodyNondeductibleRefTran { get; set; }
        public NetSuiteCustomerRefundCustomer? Customer { get; set; }
        public NetSuiteCustomerRefundCustomForm? CustomForm { get; set; }
        public NetSuiteCustomerRefundDeposit? Deposit { get; set; }
        public double? ExchangeRate { get; set; }
        public bool? ExcludeFromGLNumbering { get; set; }
        [Key]
        public string? ID { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string? Memo { get; set; }
        public NetSuiteCustomerRefundPayeeAddress? PayeeAddress { get; set; }
        public string? PayeeAddressText { get; set; }
        public NetSuiteCustomerRefundPayeeAddressList? PayeeAddressList { get; set; }
        public NetSuiteCustomerRefundPaymentOperation? PaymentOperation { get; set; }
        public NetSuiteCustomerRefundPostingPeriod? PostingPeriod { get; set; }
        public string? PrevDate { get; set; }
        public NetSuiteCustomerRefundSubsidiary? Subsidiary { get; set; }
        public bool? ToBePrinted { get; set; }
        public double? Total { get; set; }
        public string? TranDate { get; set; }
        public string? TranId { get; set; }

        //Extra fields
        [JsonIgnore]
        public DateTime? AcademicYearStartDate { get; set; }
        [JsonIgnore]
        public DateTime? AcademicYearEndDate { get; set; }
        [JsonIgnore]
        public CustomerRefundMatchType? CustomerRefundMatchType { get; set; }
        [JsonIgnore]
        public RecordActionType? RecordActionType { get; set; }
        [JsonIgnore]
        [Column(TypeName = "decimal(16,0)")]
        public decimal? UNITeStudentID { get; set; }
        [JsonIgnore]
        [Column(TypeName = "decimal(16,0)")]
        public decimal UNITeEnrolmentID { get; set; }
    }

    public enum CustomerRefundMatchType
    {
        ByAcademicYear,
        ByCustomerIDAndAmount,
        NotFound
    }
}
