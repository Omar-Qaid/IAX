using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("InventPosting")]
    public class InventPosting : Entity<long>
    {
        //----------------------------------------- Core Identity & Posting Context
        // Enum Properties
        public InventAccountType InventAccountType { get; set; } // Financial transaction target segment type

        // ==========================================================
        // Item Filtering Scope (What matches this rule)
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ItemRelation)]
        public string ItemRelation { get; set; } = string.Empty; // Code rule match context (Item ID or Group ID)

        public long CategoryRelation { get; set; } // Alternative link context pointing to EcoResCategoryRecId

        // Enum Properties
        public TableGroupAll ItemCode { get; set; } // Table (Specific Item), Group (Item Group), or All items

        // ==========================================================
        // Sub-Ledger Account Scope (Who matches this rule)
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CustVendRelation)]
        public string CustVendRelation { get; set; } = string.Empty; // Code rule match context (Cust/Vend ID or Group ID)

        // Enum Properties
        public TableGroupAll CustVendCode { get; set; } // Table, Group, or All customers/vendors

        // ==========================================================
        // Financial Targets & Strategic Ledgers
        // ==========================================================
        // Basic Properties
        public long LedgerDimension { get; set; } // Financial Main Account Combination reference anchor

        // Enum Properties
        public InventPostingCostCode CostCode { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(LedgerDimension))]
//         public virtual DimensionAttributeValueCombination? AccountCombination { get; set; }

        #endregion
    }
}
