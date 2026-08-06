using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("InventItemBarcode")]
    public class InventItemBarcode : Entity<long>
    {
        //----------------------------------------- Core Identity & Barcode Keys
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ItemBarcode)]
        public string ItemBarcode { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.BarcodeSetupId)]
        public string BarcodeSetupId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Description)]
        public string Description { get; set; } = string.Empty;

        // ==========================================================
        // Inventory Dimensions & Allocations
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.InventDimId)]
        public string InventDimId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.UnitId)]
        public string UnitId { get; set; } = string.Empty;

        public decimal Qty { get; set; } // Default quantity multiplier for barcode scan

        // ==========================================================
        // Scanning & Printing Hardware Rules
        // ==========================================================
        // Enum Properties
        public NoYes UseForInput { get; set; }
        public NoYes UseForPrinting { get; set; }
        public NoYes Blocked { get; set; }

        // ==========================================================
        // Commerce / Retail Variant Scopes
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.RetailVariantId)]
        public string RetailVariantId { get; set; } = string.Empty;

        // Enum Properties
        public NoYes RetailShowForItem { get; set; }


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
