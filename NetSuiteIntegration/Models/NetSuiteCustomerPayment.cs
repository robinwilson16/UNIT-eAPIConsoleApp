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
        public ICollection<NetSuiteLink>? Links;
        public NetSuiteCustomerPaymentAccount? Account;
        public double? Applied;
        public NetSuiteCustomerPaymentApply? Apply;
        public NetSuiteCustomerPaymentAracct? Aracct;
        public double? Balance;
        public bool? Cleared;
        public string? ClearedDate;
        public DateTime? CreatedDate;
        public NetSuiteCustomerPaymentCredit? Credit;
        public NetSuiteCustomerPaymentCurrency? Currency;
        public bool? Custbody9997AutocashAssertionField;
        public bool? Custbody9997IsForEpDd;
        public NetSuiteCustomerPaymentCustbodyAtlasNoHdn? CustbodyAtlasNoHdn;
        public NetSuiteCustomerPaymentCustbodyAtlasYesHdn? CustbodyAtlasYesHdn;
        public NetSuiteCustomerPaymentCustbodyAznCurrentUser? CustbodyAznCurrentUser;
        public string? CustbodyExternalId;
        public NetSuiteCustomerPaymentCustbodyNondeductibleRefTran? CustbodyNondeductibleRefTran;
        public NetSuiteCustomerPaymentCustomer? Customer;
        public NetSuiteCustomerPaymentCustomForm? CustomForm;
        public NetSuiteCustomerPaymentDeposit? Deposit;
        public double? ExchangeRate;
        public bool? ExcludeFromGLNumbering;
        public string? ExternalId;
        [Key]
        public string? ID;
        public DateTime? LastModifiedDate;
        public string? Memo;
        public double? Payment;
        public double? Pending;
        public NetSuiteCustomerPaymentPostingPeriod? PostingPeriod;
        public string? PrevDate;
        public NetSuiteCustomerPaymentStatus? Status;
        public NetSuiteCustomerPaymentSubsidiary? Subsidiary;
        public bool? ToBeEmailed;
        public double? Total;
        public string? TranDate;
        public string? TranId;
        public double? Unapplied;
        public NetSuiteCustomerPaymentUndepFunds? UndepFunds;
    }
}
