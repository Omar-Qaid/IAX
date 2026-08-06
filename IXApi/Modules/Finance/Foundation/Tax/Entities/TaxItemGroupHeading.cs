using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("TaxItemGroupHeading")]
    public class TaxItemGroupHeading : Entity<long>
    {
        //----------------------------------------- Core Identity & Descriptive Data
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.TaxItemGroup)]
        public string TaxItemGroup { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        // Enum Properties
        public TaxGroupSource Source { get; set; } // Core ledger matrix vs. External integrated tier markers

        // ==========================================================
        // Regulatory Trade & Reporting Declarations
        // ==========================================================
        // Enum Properties
        public EuSalesListType EuSalesListType { get; set; } // Categorization for cross-border European reporting (Items, Services, Triangulated)
    }
}
