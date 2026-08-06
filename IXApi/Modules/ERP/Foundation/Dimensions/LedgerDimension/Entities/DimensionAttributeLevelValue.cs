using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("DimensionAttributeLevelValue")]
    public class DimensionAttributeLevelValue : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        public long DimensionAttributeValueGroup { get; set; }
        public long DimensionAttributeValue { get; set; }
        public int Ordinal { get; set; }

        [Required]
        [StringLength(FieldLengths.DisplayValue)]
        public string DisplayValue { get; set; } = string.Empty;

        // ==========================================================
        // System Audit Fields
        // ==========================================================
        // Basic Properties
        public long ModifiedTransactionId { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(DimensionAttributeValue))]
//         public virtual DimensionAttributeValue? AttributeValue { get; set; }

//         [ForeignKey(nameof(DimensionAttributeValueGroup))]
//         public virtual DimensionAttributeValueGroup? ValueGroup { get; set; }

        #endregion
    }
}
