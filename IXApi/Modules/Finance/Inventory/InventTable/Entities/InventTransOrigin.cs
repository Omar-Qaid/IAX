using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("InventTransOrigin")]
    public class InventTransOrigin : Entity<long>
    {
        //----------------------------------------- Core Identity & Trace Links
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.InventTransId)]
        public string InventTransId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ItemInventDimId)]
        public string ItemInventDimId { get; set; } = string.Empty; // Inventory dimensions context for the origin header

        // ==========================================================
        // Source Document Reference Framework
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ReferenceId)]
        public string ReferenceId { get; set; } = string.Empty; // Document Number (e.g., SO-001, PO-002)

        // Enum Properties
        public InventRefType ReferenceCategory { get; set; } // Identifies source module type (Sales, Purch, Prod, etc.)

        // ==========================================================
        // Global Party Reference & Valuation Flags
        // ==========================================================
        // Basic Properties
        public long Party { get; set; } // Global Record ID relationship link to DirPartyTable

        // Enum Properties
        public NoYes IsExcludedFromInventoryValue { get; set; }

        // ==========================================================
        // System State & History Diagnostics
        // ==========================================================
        // Basic Properties
        public int SysDataStateCode { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? ReleasedProduct { get; set; }

//         [ForeignKey(nameof(ItemInventDimId))]
//         public virtual InventDim? HeaderDimensions { get; set; }

        #endregion
    }
}

