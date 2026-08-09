using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("TaxExemptCodeTable")]
    public class TaxExemptCodeTable : Entity<long>
    {
        //----------------------------------------- Core Identity & Descriptive Data
        // Basic Properties
        [Required]
        [StringLength(10)]
        public string ExemptCode { get; set; } = string.Empty; // Primary tax exemption code identifier (e.g., "EXPORT", "ZERO")

        [Required]
        [StringLength(60)]
        public string Description { get; set; } = string.Empty; // Official legal explanation or narrative printed on invoices
    }
}
