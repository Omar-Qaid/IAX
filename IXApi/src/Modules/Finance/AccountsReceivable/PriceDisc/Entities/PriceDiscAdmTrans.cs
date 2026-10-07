using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    /// <summary>Trade Agreement Journal Line / Transaction (AX PriceDiscAdmTrans).</summary>
    [Table("PriceDiscAdmTrans")]
    public class PriceDiscAdmTrans : Entity<long>
    {
        [Required]
        [StringLength(100)]
        public string JournalNum { get; set; } = string.Empty;

        [Column(TypeName = "decimal(32, 16)")]
        public decimal LineNum { get; set; }

        public PriceDiscProductCodeType ItemCode { get; set; }

        [StringLength(20)]
        public string ItemRelation { get; set; } = string.Empty;

        public PriceDiscPartyCodeType AccountCode { get; set; }

        [StringLength(20)]
        public string AccountRelation { get; set; } = string.Empty;

        public PriceType Relation { get; set; }

        public ModuleInventCustVend Module { get; set; }

        //----------------------------------------- Amounts & Pricing
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

        [StringLength(255)]
        public string Log { get; set; } = string.Empty;

        //----------------------------------------- Flags & Status
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

        //----------------------------------------- Extended Amounts
        [Column(TypeName = "decimal(32, 6)")]
        public decimal MaximumRetailPrice_In { get; set; }

        [Column(TypeName = "decimal(32, 6)")]
        public decimal SubBillFlatTierPrice { get; set; }

        //----------------------------------------- Reference Keys
        public long PriceDiscTableRef { get; set; }

        public long AgreementHeaderExt_Ru { get; set; }

        public long PricingRuleHeader { get; set; }

        public long PricingRuleLine { get; set; }

        public long PriceComponentCombination { get; set; }

        public long Partition { get; set; }

        #region Navigation Properties Row
        [ForeignKey(nameof(JournalNum))]
        public virtual PriceDiscAdmTable? PriceDiscAdmTable { get; set; }

        [ForeignKey(nameof(PriceDiscTableRef))]
        public virtual PriceDiscTable? PriceDiscTable { get; set; }

        [ForeignKey(nameof(AccountRelation))]
        public virtual CustTable? CustTable { get; set; }

        [ForeignKey(nameof(ItemRelation))]
        public virtual InventTable? InventTable { get; set; }

        [ForeignKey(nameof(InventDimId))]
        public virtual InventDim? InventDim { get; set; }
        #endregion
    }
}
