using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("InventCountGroup")]
    public class InventCountGroup : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CountGroupId)]
        public string CountGroupId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        // ==========================================================
        // Counting Frequency & Policy Parameters
        // ==========================================================
        // Basic Properties
        public int CountPeriod { get; set; } // Counting interval frequency in days

        // Enum Properties
        public InventCountCode CountCode { get; set; } // e.g., Manual, Period, Zero stock, Minimum
    }
}