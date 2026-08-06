using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;


namespace IAX.IXApi.Modules.ERP.Entities
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
