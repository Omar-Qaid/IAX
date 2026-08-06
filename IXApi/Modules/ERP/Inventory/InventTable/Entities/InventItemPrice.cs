using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("InventItemPrice")]
    public class InventItemPrice : Entity<long>
    {
        //----------------------------------------- Core Identity & Link Relations
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventDimId)]
        public string InventDimId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.VersionId)]
        public string VersionId { get; set; } = string.Empty; // Costing version identifier

        [Required]
        [StringLength(FieldLengths.PriceCalcId)]
        public string PriceCalcId { get; set; } = string.Empty; // Calculation ID trace map

        // Enum Properties
        public CostingType CostingType { get; set; } // Standard Cost vs. Planned Cost
        public PreferredPriceType PriceType { get; set; } // Cost, Sales price, or Purchase price

        // ==========================================================
        // Price Pricing Metrics & Units
        // ==========================================================
        // Basic Properties
        public decimal Price { get; set; }
        public decimal PriceQty { get; set; }
        public decimal PriceUnit { get; set; }
        public decimal Markup { get; set; }

        [Required]
        [StringLength(FieldLengths.UnitId)]
        public string UnitId { get; set; } = string.Empty;

        // Enum Properties
        public NoYes PriceAllocateMarkup { get; set; }

        // ==========================================================
        // Activation & Standard Cost Ledger Anchors
        // ==========================================================
        // Basic Properties
        public DateTime ActivationDate { get; set; }
        public DateTime StdCostTransDate { get; set; }

        [Required]
        [StringLength(FieldLengths.StdCostVoucher)]
        public string StdCostVoucher { get; set; } = string.Empty;

        // ==========================================================
        // Concurrency & System Uniqueness Tracers
        // ==========================================================
        // Basic Properties
        public Guid LastPriceUniquenessAllowance { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(InventDimId))]
//         public virtual InventDim? Dimensions { get; set; }

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? InventTable { get; set; }

//         [ForeignKey(nameof(UnitId))]
//         public virtual UnitOfMeasure? UnitOfMeasure { get; set; }

        #endregion
    }
}
