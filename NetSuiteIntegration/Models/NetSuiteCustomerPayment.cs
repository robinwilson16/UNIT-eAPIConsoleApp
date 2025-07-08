using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSuiteIntegration.Models
{
    public class NetSuiteCustomerPayment
    {
        public ICollection<NetSuiteLink>? Links { get; set; }
        public NetSuiteCustomerPaymentAccount? Account { get; set; }
        public double? Applied { get; set; }
        public NetSuiteCustomerPaymentApply? Apply { get; set; }
        public NetSuiteCustomerPaymentAracct? Aracct { get; set; }
        public double? Balance { get; set; }
        public bool? Cleared { get; set; }
        public string? ClearedDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public NetSuiteCustomerPaymentCredit? Credit { get; set; }
        public NetSuiteCustomerPaymentCurrency? Currency { get; set; }
        public bool? Custbody9997AutocashAssertionField { get; set; }
        public bool? Custbody9997IsForEpDd { get; set; }
        public NetSuiteCustomerPaymentCustbodyAtlasNoHdn? CustbodyAtlasNoHdn { get; set; }
        public NetSuiteCustomerPaymentCustbodyAtlasYesHdn? CustbodyAtlasYesHdn { get; set; }
        public NetSuiteCustomerPaymentCustbodyAznCurrentUser? CustbodyAznCurrentUser { get; set; }
        public string? CustbodyExternalId { get; set; }
        public NetSuiteCustomerPaymentCustbodyNondeductibleRefTran? CustbodyNondeductibleRefTran { get; set; }
        public NetSuiteCustomerPaymentCustomer? Customer { get; set; }
        public NetSuiteCustomerPaymentCustomForm? CustomForm { get; set; }
        public NetSuiteCustomerPaymentDeposit? Deposit { get; set; }
        public double? ExchangeRate { get; set; }
        public bool? ExcludeFromGLNumbering { get; set; }
        public string? ExternalId { get; set; }
        [Key]
        public string? ID { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string? Memo { get; set; }
        public double? Payment { get; set; }
        public double? Pending { get; set; }
        public NetSuiteCustomerPaymentPostingPeriod? PostingPeriod { get; set; }
        public string? PrevDate { get; set; }
        public NetSuiteCustomerPaymentStatus? Status { get; set; }
        public NetSuiteCustomerPaymentSubsidiary? Subsidiary { get; set; }
        public bool? ToBeEmailed { get; set; }
        public double? Total { get; set; }
        public string? TranDate { get; set; }
        public string? TranId { get; set; }
        public double? Unapplied { get; set; }
        public NetSuiteCustomerPaymentUndepFunds? UndepFunds { get; set; }
    }
}
