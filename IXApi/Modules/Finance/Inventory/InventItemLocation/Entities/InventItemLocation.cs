using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Modules.Finance.Shared.Features;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("InventItemLocation")]
    public class InventItemLocation : Entity<long>
    {
        //----------------------------------------- Core Information & Identity
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventDimId)]
        public string InventDimId { get; set; } = string.Empty;

        // ==========================================================
        // Standard Picking & Refill Controls
        // ==========================================================
        // Basic Properties
        public decimal PickingLocationMaxQty { get; set; }
        public decimal PickingLocationRefillMin { get; set; }

        // ==========================================================
        // Catch Weight (PDS) Picking & Refill Controls
        // ==========================================================
        // Basic Properties
        public decimal PdscwPickingLocationMaxQty { get; set; }
        public decimal PdscwPickingLocationRefillMin { get; set; }

        // ==========================================================
        // Warehouse Management (WMS) Directives & Fallbacks
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.WmsLocationIdDefaultReceipt)]
        public string WmsLocationIdDefaultReceipt { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.WmsLocationIdDefaultIssue)]
        public string WmsLocationIdDefaultIssue { get; set; } = string.Empty;

        // Enum Properties
        public NoYes UseWmsOrder { get; set; }
        public NoYes UseEmptyPalletLocation { get; set; }

        // ==========================================================
        // Inventory Cycle Counting Frameworks
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CountGroupId)]
        public string CountGroupId { get; set; } = string.Empty;


        #region Navigation Properties Row

//         [ForeignKey(nameof(InventDimId))]
//         public virtual InventDim? Dimensions { get; set; }

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? InventTable { get; set; }

//         [ForeignKey(nameof(CountGroupId))]
//         public virtual InventCountGroup? CountGroup { get; set; }

        #endregion
    }
}

