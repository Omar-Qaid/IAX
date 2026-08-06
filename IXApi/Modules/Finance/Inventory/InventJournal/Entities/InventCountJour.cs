using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Modules.Finance.Shared.Features;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("InventCountJour")]
    public class InventCountJour : Entity<long>
    {
        //----------------------------------------- Core Information & Identity
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.JournalId)]
        public string JournalId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventDimId)]
        public string InventDimId { get; set; } = string.Empty;

        public DateTime CountDate { get; set; }

        // ==========================================================
        // Quantities & Catch Weight Adjustments
        // ==========================================================
        // Basic Properties
        public decimal InventOnHand { get; set; }
        public decimal CountedQty { get; set; }
        public decimal PdscwQty { get; set; } // Process Manufacturing Catch Weight Quantity

        // ==========================================================
        // Ownership & Validation Statuses
        // ==========================================================
        // Basic Properties
        public long Worker { get; set; }

        // Enum Properties
        public NoYes Ok { get; set; } // Line counted/validated indicator


        #region Navigation Properties Row

//         [ForeignKey(nameof(InventDimId))]
//         public virtual InventDim? Dimensions { get; set; }

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? InventTable { get; set; }

//         [ForeignKey(nameof(JournalId))]
//         public virtual InventJournalTable? JournalHeader { get; set; }

//         [ForeignKey(nameof(Worker))]
//         public virtual IAX.IXApi.Modules.Organization.Employees.OrgEmployee? Employee { get; set; }

        #endregion
    }
}

