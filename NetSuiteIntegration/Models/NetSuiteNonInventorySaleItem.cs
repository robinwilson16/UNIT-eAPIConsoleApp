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
    public class NetSuiteNonInventorySaleItem
    {
        public ICollection<NetSuiteLink>? Links { get; set; }
        public NetSuiteNonInventorySaleItemClass? Class { get; set; }
        public NetSuiteNonInventorySaleItemCostEstimateType? CostEstimateType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public NetSuiteNonInventorySaleItemCreateRevenuePlansOn? CreateRevenuePlansOn { get; set; }
        public string? CustItem1 { get; set; }
        public string? CustItem2 { get; set; }
        [JsonPropertyName("custitem_is_po_item")]
        public bool? CustItemIsPOItem { get; set; }
        [JsonPropertyName("custitemexternalid_item")]
        public string? CustItemExternalIDItem { get; set; }
        public NetSuiteNonInventorySaleItemCustomForm? CustomForm { get; set; }
        public NetSuiteNonInventorySaleItemDeferredRevenueAccount? DeferredRevenueAccount { get; set; }
        public NetSuiteNonInventorySaleItemDepartment? Department { get; set; }
        public bool? DirectRevenuePosting { get; set; }
        public string? DisplayName { get; set; }
        public bool? EnforceMinQTYInternally { get; set; }
        public string? ExternalID { get; set; }
        [Key]
        public string? ID { get; set; }
        public bool? IncludeChildren { get; set; }
        public NetSuiteNonInventorySaleItemIncomeAccount? IncomeAccount { get; set; }
        public bool? IsFulfillable { get; set; }
        public bool? IsGCOCompliant { get; set; }
        public bool? IsInactive { get; set; }
        public bool? IsOnline { get; set; }
        public string? ItemID { get; set; }
        //public NetSuiteNonInventorySaleItemItemType? ItemType { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public NetSuiteNonInventorySaleItemLocation? Location { get; set; }
        public NetSuiteNonInventorySaleItemPrice? Price { get; set; }
        public NetSuiteNonInventorySaleItemRevenueRecognitionRule? RevenueRecognitionRule { get; set; }
        public NetSuiteNonInventorySaleItemRevRecForecastRule? RevRecForecastRule { get; set; }
        public string? SalesDescription { get; set; }
        public NetSuiteNonInventorySaleItemSubsidiary? Subsidiary { get; set; }
        public NetSuiteNonInventorySaleItemTaxSchedule? TaxSchedule { get; set; }
        public NetSuiteNonInventorySaleItemTranslations? Translations { get; set; }
        public bool? UseMarginalRates { get; set; }
        public bool? VSOEDelivered { get; set; }
        public NetSuiteNonInventorySaleItemVSOESopGroup? VSOESopGroup { get; set; }

        //Extra fields
        [JsonIgnore]
        public DateTime? AcademicYearStartDate { get; set; }
        [JsonIgnore]
        public DateTime? AcademicYearEndDate { get; set; }
        [JsonIgnore]
        public NonInventorySaleItemMatchType? NonInventorySaleItemMatchType { get; set; }
        [JsonIgnore]
        public RecordActionType? RecordActionType { get; set; }

        [Column(TypeName = "decimal(16,0)")]
        public decimal UNITeCourseID { get; set; }
    }

    public enum NonInventorySaleItemMatchType
    {
        ByCourseCode,
        NotFound
    }
}
