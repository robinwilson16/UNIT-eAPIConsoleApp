using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UNITe.Business.Helper;
using static NetSuiteIntegration.Models.SharedEnum;

namespace NetSuiteIntegration.Models
{
    public class NetSuiteCreditMemo
    {
        public ICollection<NetSuiteLink>? Links { get; set; }
        public NetSuiteCreditMemoAccount? Account { get; set; }
        public double? AmountPaid { get; set; }
        public double? AmountRemaining { get; set; }
        public double? Applied { get; set; }
        public NetSuiteCreditMemoApply? Apply { get; set; }
        public string? AsOfDate { get; set; }
        public string? BillAddress { get; set; }
        public NetSuiteCreditMemoBillAddressList? BillAddressList { get; set; }
        public NetSuiteCreditMemoBillingAddress? BillingAddress { get; set; }
        [JsonPropertyName("billingAddress_text")]
        public string? BillingAddressText { get; set; }
        public bool? CanHaveStackable { get; set; }
        public NetSuiteCreditMemoClass? Class { get; set; }
        public DateTime? CreatedDate { get; set; }
        public NetSuiteCreditMemoCurrency? Currency { get; set; }
        [JsonPropertyName("custbody_15699_exclude_from_ep_process")]
        public bool? Custbody15699ExcludeFromEPProcess { get; set; }
        [JsonPropertyName("custbody_atlas_exist_cust_hdn")]
        public NetSuiteCreditMemoCustbodyAtlasExistCustHdn? CustbodyAtlasExistCustHdn { get; set; }
        [JsonPropertyName("custbody_atlas_new_cust_hdn")]
        public NetSuiteCreditMemoCustbodyAtlasNewCustHdn? CustbodyAtlasNewCustHdn { get; set; }
        [JsonPropertyName("custbody_atlas_no_hdn")]
        public NetSuiteCreditMemoCustbodyAtlasNoHdn? CustbodyAtlasNoHdn { get; set; }
        [JsonPropertyName("custbody_atlas_yes_hdn")]
        public NetSuiteCreditMemoCustbodyAtlasYesHdn? CustbodyAtlasYesHdn { get; set; }
        [JsonPropertyName("custbody_emea_transaction_type")]
        public string? CustbodyEmeaTransactionType { get; set; }
        [JsonPropertyName("custbody_esc_created_date")]
        public string? CustbodyEscCreatedDate { get; set; }
        [JsonPropertyName("custbody_esc_last_modified_date")]
        public string? CustbodyEscLastModifiedDate { get; set; }
        [JsonPropertyName("custbody_external_id")]
        public string? CustbodyExternalID { get; set; }
        [JsonPropertyName("custbody_f3_intercompany_internal_vb")]
        public NetSuiteCreditMemoCustbodyF3IntercompanyInternalVb? CustbodyF3IntercompanyInternalVb { get; set; }
        [JsonPropertyName("custbody_nondeductible_ref_tran")]
        public NetSuiteCreditMemoCustbodyNondeductibleRefTran? CustbodyNondeductibleRefTran { get; set; }
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
        [JsonPropertyName("custbody_znc_gbp_equiv_net")]
        public double? CustbodyZncGbpEquivNet { get; set; }
        [JsonPropertyName("custbody_znc_gbp_equiv_total")]
        public double? CustbodyZncGbpEquivTotal { get; set; }
        [JsonPropertyName("custbody_znc_gbp_equiv_vat")]
        public double? CustbodyZncGbpEquivVat { get; set; }
        public NetSuiteCreditMemoCustomForm? CustomForm { get; set; }
        public NetSuiteCreditMemoDepartment? Department { get; set; }
        public double? DiscountTotal { get; set; }
        public string? Email { get; set; }
        public NetSuiteCreditMemoEntity? Entity { get; set; }
        public double? EstGrossProfit { get; set; }
        public double? EstGrossProfitPercent { get; set; }
        public double? ExchangeRate { get; set; }
        public bool? ExcludeFromGLNumbering { get; set; }
        public string? ExternalID { get; set; }
        [Key]
        public string? ID { get; set; }
        public NetSuiteCreditMemoItem? Item { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public NetSuiteCreditMemoLocation? Location { get; set; }
        public string? Memo { get; set; }
        public string? Originator { get; set; }
        public NetSuiteCreditMemoPostingPeriod? PostingPeriod { get; set; }
        public string? PrevDate { get; set; }
        public string? SalesEffectiveDate { get; set; }
        public NetSuiteCreditMemoSalesTeam? SalesTeam { get; set; }
        public string? ShipAddress { get; set; }
        public NetSuiteCreditMemoShipAddressList? ShipAddressList { get; set; }
        public bool? ShipIsResidential { get; set; }
        public bool? ShipOverride { get; set; }
        public NetSuiteCreditMemoShippingAddress? ShippingAddress { get; set; }
        [JsonPropertyName("shippingAddress_text")]
        public string? ShippingAddressText { get; set; }
        public NetSuiteCreditMemoSource? Source { get; set; }
        public NetSuiteCreditMemoStatus? Status { get; set; }
        public NetSuiteCreditMemoSubsidiary? Subsidiary { get; set; }
        public double? Subtotal { get; set; }
        public bool? ToBeEmailed { get; set; }
        public bool? ToBeFaxed { get; set; }
        public bool? ToBePrinted { get; set; }
        public double? Total { get; set; }
        public double? TotalCostEstimate { get; set; }
        public string? TranDate { get; set; }
        public string? TranID { get; set; }
        public double? Unapplied { get; set; }

        //Extra fields
        [JsonIgnore]
        public DateTime? AcademicYearStartDate { get; set; }
        [JsonIgnore]
        public DateTime? AcademicYearEndDate { get; set; }
        [JsonIgnore]
        public CreditMemoMatchType? CreditMemoMatchType { get; set; }
        [JsonIgnore]
        public RecordActionType? RecordActionType { get; set; }
        [JsonIgnore]
        [Column(TypeName = "decimal(16,0)")]
        public decimal? UNITeStudentID { get; set; }
        [JsonIgnore]
        [Column(TypeName = "decimal(16,0)")]
        public decimal UNITeEnrolmentID { get; set; }
    }

    public enum CreditMemoMatchType
    {
        ByCourseCode,
        NotFound
    }
}
