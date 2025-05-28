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
    public class NetSuiteInvoice
    {
        public ICollection<NetSuiteLink>? Links { get; set; }
        public NetSuiteInvoiceAccount? Account { get; set; }
        public double? AmountPaid { get; set; }
        public double? AmountRemaining { get; set; }
        public double? AmountRemainingTotalBox { get; set; }
        public NetSuiteInvoiceApprovalStatus? ApprovalStatus { get; set; }
        public string? AsOfDate { get; set; }
        public string? BillAddress { get; set; }
        public NetSuiteInvoiceBillAddressList? BillAddressList { get; set; }
        public NetSuiteInvoiceBillingAddress? BillingAddress { get; set; }
        public string? BillingAddressText { get; set; }
        public bool? CanHaveStackable { get; set; }
        public NetSuiteInvoiceClass? Class { get; set; }
        public DateTime? CreatedDate { get; set; }
        public NetSuiteInvoiceCurrency? Currency { get; set; }
        [JsonPropertyName("custbody_15699_exclude_from_ep_process")]
        public bool? Custbody15699ExcludeFromEPProcess { get; set; }
        [JsonPropertyName("custbody_atlas_exist_cust_hdn")]
        public NetSuiteInvoiceCustbodyAtlasExistCustHdn? CustbodyAtlasExistCustHdn { get; set; }
        [JsonPropertyName("custbody_atlas_new_cust_hdn")]
        public NetSuiteInvoiceCustbodyAtlasNewCustHdn? CustbodyAtlasNewCustHdn { get; set; }
        [JsonPropertyName("custbody_atlas_no_hdn")]
        public NetSuiteInvoiceCustbodyAtlasNoHdn? CustbodyAtlasNoHdn { get; set; }
        [JsonPropertyName("custbody_atlas_yes_hdn")]
        public NetSuiteInvoiceCustbodyAtlasYesHdn? CustbodyAtlasYesHdn { get; set; }
        [JsonPropertyName("custbody_emea_transaction_type")]
        public string? CustbodyEmeaTransactionType { get; set; }
        [JsonPropertyName("custbody_esc_created_date")]
        public string? CustbodyEscCreatedDate { get; set; }
        [JsonPropertyName("custbody_esc_last_modified_date")]
        public string? CustbodyEscLastModifiedDate { get; set; }
        [JsonPropertyName("custbody_external_id")]
        public string? CustbodyExternalID { get; set; }
        [JsonPropertyName("custbody_f3_next_approval_by")]
        public NetSuiteInvoiceCustbodyF3NextApprovalBy? CustbodyF3NextApprovalBy { get; set; }
        [JsonPropertyName("custbody_nondeductible_ref_tran")]
        public NetSuiteInvoiceCustbodyNondeductibleRefTran? CustbodyNondeductibleRefTran { get; set; }
        [JsonPropertyName("custbody_report_timestamp")]
        public string? CustbodyReportTimestamp { get; set; }
        [JsonPropertyName("custbody_sii_article_61d")]
        public bool? CustbodySiiArticle61d { get; set; }
        [JsonPropertyName("custbody_sii_article_72_73")]
        public bool? CustbodySiiArticle7273 { get; set; }
        [JsonPropertyName("custbody_sii_is_third_party")]
        public bool? CustbodySiiIsThirdParty { get; set; }
        [JsonPropertyName("custbody_sii_not_reported_in_time")]
        public bool? CustbodySiiNotReportedInTime { get; set; }
        [JsonPropertyName("custbody_stc_amount_after_discount")]
        public double? CustbodyStcAmountAfterDiscount { get; set; }
        [JsonPropertyName("custbody_stc_tax_after_discount")]
        public double? CustbodyStcTaxAfterDiscount { get; set; }
        [JsonPropertyName("custbody_stc_total_after_discount")]
        public double? CustbodyStcTotalAfterDiscount { get; set; }
        [JsonPropertyName("custbody_znc_gbp_equiv_net")]
        public double? CustbodyZncGbpEquivNet { get; set; }
        [JsonPropertyName("custbody_znc_gbp_equiv_total")]
        public double? CustbodyZncGbpEquivTotal { get; set; }
        [JsonPropertyName("custbody_znc_gbp_equiv_vat")]
        public double? CustbodyZncGbpEquivVat { get; set; }
        public NetSuiteInvoiceCustomForm? CustomForm { get; set; }
        public double? DiscountTotal { get; set; }
        public string? DueDate { get; set; }
        public string? Email { get; set; }
        public NetSuiteInvoiceEntity? Entity { get; set; }
        public double? EstGrossProfit { get; set; }
        public double? EstGrossProfitPercent { get; set; }
        public double? ExchangeRate { get; set; }
        public bool? ExcludeFromGLNumbering { get; set; }
        public NetSuiteInvoiceExpCost? ExpCost { get; set; }
        public string? ExternalID { get; set; }
        [Key]
        public string? ID { get; set; }
        public NetSuiteInvoiceInstallment? Installment { get; set; }
        public NetSuiteInvoiceItem? Item { get; set; }
        public NetSuiteInvoiceItemCost? ItemCost { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public NetSuiteInvoiceLocation? Location { get; set; }
        public string? Memo { get; set; }
        public NetSuiteInvoiceNextApprover? NextApprover { get; set; }
        public string? Originator { get; set; }
        public bool? OverrideInstallments { get; set; }
        public NetSuiteInvoicePostingPeriod? PostingPeriod { get; set; }
        public string? PrevDate { get; set; }
        public string? SalesEffectiveDate { get; set; }
        public NetSuiteInvoiceSalesTeam? SalesTeam { get; set; }
        public string? ShipAddress { get; set; }
        public NetSuiteInvoiceShipAddressList? ShipAddressList { get; set; }
        public bool? ShipIsResidential { get; set; }
        public bool? ShipOverride { get; set; }
        public NetSuiteInvoiceShippingAddress? ShippingAddress { get; set; }
        [JsonPropertyName("shippingAddress_text")]
        public string? ShippingAddressText { get; set; }
        public NetSuiteInvoiceSource? Source { get; set; }
        public NetSuiteInvoiceStatus? Status { get; set; }
        public NetSuiteInvoiceSubsidiary? Subsidiary { get; set; }
        public double? Subtotal { get; set; }
        public NetSuiteInvoiceTime? Time { get; set; }
        public bool? ToBeEmailed { get; set; }
        public bool? ToBeFaxed { get; set; }
        public bool? ToBePrinted { get; set; }
        public double? Total { get; set; }
        public double? TotalCostEstimate { get; set; }
        public string? TranDate { get; set; }
        public string? TranID { get; set; }

        //Extra fields
        [JsonIgnore]
        public DateTime? AcademicYearStartDate { get; set; }
        [JsonIgnore]
        public DateTime? AcademicYearEndDate { get; set; }
        [JsonIgnore]
        public InvoiceMatchType? InvoiceMatchType { get; set; }
        [JsonIgnore]
        public RecordActionType? RecordActionType { get; set; }
        [JsonIgnore]
        [Column(TypeName = "decimal(16,0)")]
        public decimal? UNITeStudentID { get; set; }
        [JsonIgnore]
        [Column(TypeName = "decimal(16,0)")]
        public decimal UNITeEnrolmentID { get; set; }
    }

    public enum InvoiceMatchType
    {
        ByAcademicYear,
        ByCustomerIDAndAmount,
        ByEmail,
        NotFound
    }
}
