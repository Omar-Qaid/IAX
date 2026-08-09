using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;


namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("DimensionAttributeValueGroup")]
    public class DimensionAttributeValueGroup : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        public long DimensionHierarchy { get; set; }
        public int Levels { get; set; }

        // ==========================================================
        // Cryptographic Hashing Contexts
        // ==========================================================
        // Basic Properties
        public byte[]? Hash { get; set; } // varbinary mapping supporting Nullable = YES
        public int HashVersion { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(DimensionHierarchy))]
//         public virtual DimensionHierarchy? Hierarchy { get; set; }

        #endregion
    }
}

