using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UNITe.Business.Helper;

namespace NetSuiteIntegration.Models
{
    public class NetSuiteInvoiceItemDetail
    {
        public ICollection<NetSuiteLink>? Links { get; set; }
        public NetSuiteInvoiceItemDetailAccount? Account { get; set; }
        public double? Amount { get; set; }
        public NetSuiteInvoiceItemDetailClass? Class { get; set; }
        public double? CostEstimate { get; set; }
        public double? CostEstimateRate { get; set; }
        public NetSuiteInvoiceItemDetailCostEstimateType? CostEstimateType { get; set; }
        [JsonPropertyName("custcol1")]
        public string? Custcol1 { get; set; }
        [JsonPropertyName("custcol2")]
        public string? Custcol2 { get; set; }
        [JsonPropertyName("custcol_2663_isperson")]
        public bool? Custcol2663IsPerson { get; set; }
        [JsonPropertyName("custcol_5892_eutriangulation")]
        public bool? Custcol5892EUTriangulation { get; set; }
        [JsonPropertyName("custcol_statistical_value_base_curr")]
        public double? CustcolStatisticalValueBaseCurr { get; set; }
        public NetSuiteInvoiceItemDetailDepartment? Department { get; set; }
        public string? Description { get; set; }
        public NetSuiteInvoiceItemDetailItem? Item { get; set; }
        public NetSuiteInvoiceItemDetailItemSubtype? ItemSubtype { get; set; }
        public NetSuiteInvoiceItemDetailItemType? ItemType { get; set; }
        public int? Line { get; set; }
        public NetSuiteInvoiceItemDetailLocation? Location { get; set; }
        public bool? Marginal { get; set; }
        public NetSuiteInvoiceItemDetailPrice? Price { get; set; }
        public bool? PrintItems { get; set; }
        public double? Quantity { get; set; }
        public double? QuantityOnHand { get; set; }
        public double? Rate { get; set; }
    }
}
