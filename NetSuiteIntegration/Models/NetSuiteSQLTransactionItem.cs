using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetSuiteIntegration.Models
{
    public class NetSuiteSQLTransactionItem
    {
        public ICollection<NetSuiteLink>? Links { get; set; }
        [JsonConverter(typeof(AbbrevTypeConverter))]
        public AbbrevType? AbbrevType { get; set; }
        public string? ApprovalStatus { get; set; }
        public string? AsOfDate { get; set; }
        public string? BalSegStatus { get; set; }
        public string? BillingAddress { get; set; }
        public string? BillingStatus { get; set; }
        public string? Compliant { get; set; }
        public string? CreatedDate { get; set; }
        public string? CreatedFromMerge { get; set; }
        public string? Currency { get; set; }
        [JsonPropertyName("custbody_15699_exclude_from_ep_process")]
        public string? Custbody15699ExcludeFromEpProcess { get; set; }
        [JsonPropertyName("custbody_atlas_svcs_mm_json")]
        public string? CustbodyAtlasSvcsMmJson { get; set; }
        [JsonPropertyName("custbody_f3_next_approval_by")]
        public string? CustbodyF3NextApprovalBy { get; set; }
        [JsonPropertyName("custbody_report_timestamp")]
        public string? CustbodyReportTimestamp { get; set; }
        [JsonPropertyName("custbody_sii_article_61d")]
        public string? CustbodySiiArticle61d { get; set; }
        [JsonPropertyName("custbody_sii_article_72_73")]
        public string? CustbodySiiArticle7273 { get; set; }
        [JsonPropertyName("custbody_sii_is_third_party")]
        public string? CustbodySiiIsThirdParty { get; set; }
        [JsonPropertyName("custbody_sii_not_reported_in_time")]
        public string? CustbodySiiNotReportedInTime { get; set; }
        [JsonPropertyName("custbody_stc_amount_after_discount")]
        public string? CustbodyStcAmountAfterDiscount { get; set; }
        [JsonPropertyName("custbody_stc_tax_after_discount")]
        public string? CustbodyStcTaxAfterDiscount { get; set; }
        [JsonPropertyName("custbody_stc_total_after_discount")]
        public string? CustbodyStcTotalAfterDiscount { get; set; }
        public string? CustomForm { get; set; }
        public string? CustomType { get; set; }
        public string? DaysOpen { get; set; }
        public string? DaysOverdueSearch { get; set; }
        public string? DueDate { get; set; }
        public string? Email { get; set; }
        public string? Entity { get; set; }
        public string? EstGrossProfit { get; set; }
        public string? EstGrossProfitPercent { get; set; }
        public string? ExchangeRate { get; set; }
        public string? ExternalID { get; set; }
        public string? ForeignAmountPaid { get; set; }
        public string? ForeignAmountUnpaid { get; set; }
        public string? ForeignTotal { get; set; }
        [Key]
        public string? ID { get; set; }
        public string? IncludeInForecast { get; set; }
        public string? InterCoAdj { get; set; }
        public string? IsFinchRG { get; set; }
        public string? IsReversal { get; set; }
        public string? LastModifiedDate { get; set; }
        public string? Memo { get; set; }
        public string? Memorized { get; set; }
        public string? MergedIntoNewArrangements { get; set; }
        public string? NeedsBill { get; set; }
        public string? NextApprover { get; set; }
        public string? Nexus { get; set; }
        public string? Number { get; set; }
        public string? OrdPicked { get; set; }
        public string? PaymenthOld { get; set; }
        public string? PaymentLink { get; set; }
        public string? Posting { get; set; }
        public string? PostingPeriod { get; set; }
        public string? PrevDate { get; set; }
        public string? PrintedPickingTicket { get; set; }
        public string? RecordType { get; set; }
        public string? ShipComplete { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Source { get; set; }
        public string? Status { get; set; }
        public string? ToBePrinted { get; set; }
        public string? TotalCostEstimate { get; set; }
        public string? TranDate { get; set; }
        public string? TranDisplayName { get; set; }
        public string? TranID { get; set; }
        public string? TransactionNumber { get; set; }
        public string? Type { get; set; }
        public string? TypeBasedDocumentNumber { get; set; }
        public string? UseRevenueArrangement { get; set; }
        public string? VisibleToCustomer { get; set; }
        public string? Void { get; set; }
        public string? Voided { get; set; }
    }

    public enum AbbrevType
    {
        [Display(Name = "Invoice")]
        INV,
        [Display(Name = "Credit Memo")]
        CREDMEM,
        [Display(Name = "Customer Refund")]
        RFND,
        [Display(Name = "General Journal")]
        GENJRNL,
        PMT,
        [Display(Name = "Bill")]
        BILL
    }

    public class AbbrevTypeConverter : JsonConverter<AbbrevType?>
    {
        public override AbbrevType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
                return null;

            // Try to match enum name
            if (Enum.TryParse<AbbrevType>(value, out var result))
                return result;

            // Try to match Display(Name)
            foreach (var field in typeof(AbbrevType).GetFields())
            {
                var display = field.GetCustomAttribute<DisplayAttribute>();
                if (display != null && display.Name == value)
                    return (AbbrevType)field.GetValue(null);
            }

            throw new JsonException($"Unable to convert \"{value}\" to AbbrevType.");
        }

        public override void Write(Utf8JsonWriter writer, AbbrevType? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var field = typeof(AbbrevType).GetField(value.ToString());
            var display = field.GetCustomAttribute<DisplayAttribute>();
            writer.WriteStringValue(display?.Name ?? value.ToString());
        }
    }
}
