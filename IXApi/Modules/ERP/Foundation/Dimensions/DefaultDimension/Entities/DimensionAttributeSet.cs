using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("DimensionAttributeSet")]
    public class DimensionAttributeSet : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.BaseEnumTypeName)]
        public string BaseEnumTypeName { get; set; } = string.Empty;

        // Enum Properties
        public int BaseEnumType { get; set; } // Map to system metadata Enum types if needed

        // ==========================================================
        // Cryptographic Hashing Contexts
        // ==========================================================
        // Basic Properties
        public byte[]? Hash { get; set; } // varbinary mapping supporting Nullable = YES
        public int HashVersion { get; set; }
    }
}