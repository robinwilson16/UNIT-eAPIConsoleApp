using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UNITe.Business.Helper;
using static NetSuiteIntegration.Models.SharedEnum;

namespace NetSuiteIntegration.Models
{
    [Keyless]
    public class NetSuiteCreditMemoItemDetail
    {
        public ICollection<NetSuiteLink>? Links { get; set; }
        public NetSuiteCreditMemoItemDetailAccount? Account { get; set; }
        public double? Amount { get; set; }
        public double? CostEstimate { get; set; }
        public double? CostEstimateRate { get; set; }
        public NetSuiteCreditMemoItemDetailCostEstimateType? CostEstimateType { get; set; }
        public string? Custcol1 { get; set; }
        public string? Custcol2 { get; set; }
        [JsonPropertyName("custcol_2663_isperson")]
        public bool? Custcol2663IsPerson { get; set; }
        [JsonPropertyName("custcol_5892_eutriangulation")]
        public bool? Custcol5892EUTriangulation { get; set; }
        [JsonPropertyName("custcol_statistical_value_base_curr")]
        public double? CustcolStatisticalValueBaseCurr { get; set; }
        public NetSuiteCreditMemoItemDetailDepartment? Department { get; set; }
        public string? Description { get; set; }
        public bool? IsDropShipment { get; set; }
        public NetSuiteCreditMemoItemDetailItem? Item { get; set; }
        public NetSuiteCreditMemoItemDetailItemSubtype? ItemSubtype { get; set; }
        public NetSuiteCreditMemoItemDetailItemType? ItemType { get; set; }
        public int? Line { get; set; }
        public NetSuiteCreditMemoItemDetailLocation? Location { get; set; }
        public bool? Marginal { get; set; }
        public NetSuiteCreditMemoItemDetailPrice? Price { get; set; }
        public bool? PrintItems { get; set; }
        public double? Quantity { get; set; }

        //Extra Fields
        [JsonIgnore]
        public RecordActionType? RecordActionType { get; set; }
        [JsonIgnore]
        public bool? IsMainCreditMemoLine { get; set; }
    }
}
