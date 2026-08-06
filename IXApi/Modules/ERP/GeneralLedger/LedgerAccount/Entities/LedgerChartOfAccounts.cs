using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("LedgerChartOfAccounts")]
    public class LedgerChartOfAccounts : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Description)]
        public string Description { get; set; } = string.Empty;

        // ==========================================================
        // Structural Validation Masks
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.MainAccountFormatMask)]
        public string MainAccountFormatMask { get; set; } = string.Empty; // e.g., "######" or "##-###"
    }
}