using System;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDisc
{
    public class PriceDiscAdmTransDto : EntityDto<long>
    {
        [Required]
        [StringLength(100)]
        public string JournalNum { get; set; } = string.Empty;

        public decimal LineNum { get; set; }

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

        [StringLength(255)]
        public string Log { get; set; } = string.Empty;

        public int AllocateMarkup { get; set; }

        public int DifferentFromPosted { get; set; }

        public int GenericCurrency { get; set; }

        public int MustBeDeleted { get; set; }

        public int SearchAgain { get; set; }

        public int PriceApplyAdjustment { get; set; }

        public int UnitAppliesToAll { get; set; }

        public int IsGupTradeAgreement { get; set; }

        public int PricingAttributesHeaderAreMatched { get; set; }

        public int PricingAttributesLineAreMatched { get; set; }

        public int InventBaileeFreeDays_Ru { get; set; }

        public decimal MaximumRetailPrice_In { get; set; }

        public decimal SubBillFlatTierPrice { get; set; }

        public long PriceDiscTableRef { get; set; }

        public long AgreementHeaderExt_Ru { get; set; }

        public long PricingRuleHeader { get; set; }

        public long PricingRuleLine { get; set; }

        public long PriceComponentCombination { get; set; }

    }
}
