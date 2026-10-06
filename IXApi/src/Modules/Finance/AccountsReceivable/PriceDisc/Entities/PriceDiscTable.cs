using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    [Table("PriceDiscTable")]
    public class PriceDiscTable : Entity<long>
    {
        //----------------------------------------- Item & Customer Relations
        public PriceDiscProductCodeType ItemCode { get; set; }

        [StringLength(20)]
        public string ItemRelation { get; set; } = string.Empty;

        public PriceDiscPartyCodeType AccountCode { get; set; }

        [StringLength(20)]
        public string AccountRelation { get; set; } = string.Empty;

        public PriceType Relation { get; set; }

        public ModuleInventCustVend Module { get; set; }

        //----------------------------------------- Amount & Price Specifications
        [Column(TypeName = "decimal(32, 6)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(32, 6)")]
        public decimal Markup { get; set; }

        [Column(TypeName = "decimal(32, 6)")]
        public decimal Percent1 { get; set; }

        [Column(TypeName = "decimal(32, 6)")]
        public decimal Percent2 { get; set; }

        [Column(TypeName = "decimal(32, 12)")]
        public decimal PriceUnit { get; set; }

        [Column(TypeName = "decimal(32, 6)")]
        public decimal QuantityAmountFrom { get; set; }

        [Column(TypeName = "decimal(32, 6)")]
        public decimal QuantityAmountTo { get; set; }

        //----------------------------------------- Dates & Validity
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public int CalendarDays { get; set; }

        public int DeliveryTime { get; set; }

        public int DisregardLeadTime { get; set; }

        //----------------------------------------- Codes & Identification
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

        //----------------------------------------- Flags & Settings
        public int AllocateMarkup { get; set; }

        public int GenericCurrency { get; set; }

        public int SearchAgain { get; set; }

        public int PriceApplyAdjustment { get; set; }

        public int UnitAppliesToAll { get; set; }

        public int IsGupTradeAgreement { get; set; }

        public int McrPriceDiscGroupType { get; set; }

        public int InventBaileeFreeDays_Ru { get; set; }

        //----------------------------------------- Amounts (Extended)
        [Column(TypeName = "decimal(32, 6)")]
        public decimal MaximumRetailPrice_In { get; set; }

        [Column(TypeName = "decimal(32, 6)")]
        public decimal McrFixedAmountCur { get; set; }

        [Column(TypeName = "decimal(32, 6)")]
        public decimal SubBillFlatTierPrice { get; set; }

        //----------------------------------------- Reference Keys
        public long AgreementHeaderExt_Ru { get; set; }

        public long OriginalPriceDiscAdmTransRecId { get; set; }

        public long PricingRuleHeader { get; set; }

        public long PricingRuleLine { get; set; }

        public long PriceComponentCombination { get; set; }

        public long ApplicabilityId { get; set; }

        public long Partition { get; set; }

        #region Navigation Properties Row
        [ForeignKey(nameof(AccountRelation))]
        public virtual CustTable? CustTable { get; set; }

        [ForeignKey(nameof(ItemRelation))]
        public virtual InventTable? InventTable { get; set; }

        [ForeignKey(nameof(InventDimId))]
        public virtual InventDim? InventDim { get; set; }
        #endregion
    }
}
