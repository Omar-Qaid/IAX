using System;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class PriceDiscTableDto : EntityDto<long>
    {
        public PriceDiscProductCodeType ItemCode { get; set; }

        [StringLength(20)]
        public string ItemRelation { get; set; } = string.Empty;

        public PriceDiscPartyCodeType AccountCode { get; set; }

        [StringLength(20)]
        public string AccountRelation { get; set; } = string.Empty;

        public PriceType Relation { get; set; }

        public ModuleInventCustVend Module { get; set; }

        public decimal Amount { get; set; }

        public decimal Markup { get; set; }

        public decimal Percent1 { get; set; }

        public decimal Percent2 { get; set; }

        public decimal PriceUnit { get; set; }

        public decimal QuantityAmountFrom { get; set; }

        public decimal QuantityAmountTo { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public int CalendarDays { get; set; }

        public int DeliveryTime { get; set; }

        public int DisregardLeadTime { get; set; }

        [StringLength(FieldLengths.CurrencyCode)]
        public string Currency { get; set; } = string.Empty;

        [StringLength(FieldLengths.UnitId)]
        public string UnitId { get; set; } = string.Empty;

        [StringLength(FieldLengths.InventDimId)]
        public string InventDimId { get; set; } = string.Empty;

        [StringLength(10)]
        public string Agreement { get; set; } = string.Empty;

        [StringLength(10)]
        public string PriceGroup { get; set; } = string.Empty;

        [StringLength(10)]
        public string PdsCalculationId { get; set; } = string.Empty;

        [StringLength(10)]
        public string McrMerchandisingEventId { get; set; } = string.Empty;

        public int AllocateMarkup { get; set; }

        public int GenericCurrency { get; set; }

        public int SearchAgain { get; set; }

        public int PriceApplyAdjustment { get; set; }

        public int UnitAppliesToAll { get; set; }

        public int IsGupTradeAgreement { get; set; }

        public int McrPriceDiscGroupType { get; set; }

        public int InventBaileeFreeDays_Ru { get; set; }

        public decimal MaximumRetailPrice_In { get; set; }

        public decimal McrFixedAmountCur { get; set; }

        public decimal SubBillFlatTierPrice { get; set; }

        public long AgreementHeaderExt_Ru { get; set; }

        public long OriginalPriceDiscAdmTransRecId { get; set; }

        public long PricingRuleHeader { get; set; }

        public long PricingRuleLine { get; set; }

        public long PriceComponentCombination { get; set; }

        public long ApplicabilityId { get; set; }

        public long Partition { get; set; }
    }
}
