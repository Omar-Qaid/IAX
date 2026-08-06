using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("MainAccountCategory")]
    public class MainAccountCategory : Entity<long>
    {
        //----------------------------------------- Core Identity & Descriptive Data
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.AccountCategory)]
        public string AccountCategory { get; set; } = string.Empty; // Unique human-readable category key name

        [Required]
        [StringLength(FieldLengths.Description)]
        public string Description { get; set; } = string.Empty; // Descriptive label of the accounting category

        // ==========================================================
        // Financial Framework & Reporting Layouts
        // ==========================================================
        // Basic Properties
        public int AccountCategoryRef { get; set; }          // Internal system surrogate lookup mapping link
        public int AccountCategoryDisplayOrder { get; set; } // Precision layout weight sequence index for sorting reports

        // Enum Properties
        public MainAccountType AccountType { get; set; } // 0: Asset, 1: Liability, 2: Equity, 3: Revenue, 4: Expense


        #region Lifecycle Controls

        // Enum Properties
        public NoYes Closed { get; set; } // Flag explicitly disabling further account associations to this category

        #endregion
    }
}