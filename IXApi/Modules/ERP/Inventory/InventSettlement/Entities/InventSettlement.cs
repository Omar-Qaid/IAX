using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("InventSettlement")]
    public class InventSettlement : Entity<long>
    {
        //----------------------------------------- Core Identity & Link Relations
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.SettleTransId)]
        public string SettleTransId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventTransId)]
        public string InventTransId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ItemGroupId)]
        public string ItemGroupId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Voucher)]
        public string Voucher { get; set; } = string.Empty;

        public DateTime TransDate { get; set; }
        public long TransRecId { get; set; }

        // ==========================================================
        // Settled Financial Amounts & Quantities
        // ==========================================================
        // Basic Properties
        public decimal QtySettled { get; set; }
        public decimal CostAmountSettled { get; set; }
        public decimal CostAmountAdjustment { get; set; }
        public decimal PdscwSettled { get; set; } // Catch Weight Process Manufacturing Metric

        // Enum Properties
        public InventSettleType SettleType { get; set; } // Financial close vs. Recalculation adjustment
        public InventModel SettleModel { get; set; } // Inventory valuation rule mapping

        // ==========================================================
        // General Ledger Posting Dimensions
        // ==========================================================
        // Basic Properties
        public long BalanceSheetLedgerDimension { get; set; }
        public long OperationsLedgerDimension { get; set; }
        public long DefaultDimension { get; set; }

        // Enum Properties
        public LedgerPostingType BalanceSheetPosting { get; set; }
        public LedgerPostingType OperationsPosting { get; set; }

        // ==========================================================
        // Landed Cost & Extensibility Extensions
        // ==========================================================
        // Basic Properties
        public long ItmCostTransRecId { get; set; } // Landed Cost reference map marker

        // ==========================================================
        // Lifecycle History & Audit Anchors
        // ==========================================================
        // Basic Properties
        public DateTime TransBeginTime { get; set; }
        public int TransBeginTimeTzId { get; set; }
        public int SysDataStateCode { get; set; }

        // Enum Properties
        public NoYes Posted { get; set; }
        public NoYes Cancelled { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(BalanceSheetLedgerDimension))]
//         public virtual DimensionAttributeValueCombination? BalanceSheetAccountCombination { get; set; }

//         [ForeignKey(nameof(OperationsLedgerDimension))]
//         public virtual DimensionAttributeValueCombination? OperationsAccountCombination { get; set; }

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? CostCenterDimensionSet { get; set; }

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? InventTable { get; set; }

//         [ForeignKey(nameof(ItemGroupId))]
//         public virtual InventItemGroup? InventItemGroup { get; set; }

        #endregion
    }
}
