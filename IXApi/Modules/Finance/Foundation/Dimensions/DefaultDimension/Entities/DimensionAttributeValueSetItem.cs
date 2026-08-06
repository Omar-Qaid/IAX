using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("DimensionAttributeValueSetItem")]
    public class DimensionAttributeValueSetItem : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        public long DimensionAttributeValueSet { get; set; }
        public long DimensionAttributeValue { get; set; }

        [Required]
        [StringLength(FieldLengths.DisplayValue)]
        public string DisplayValue { get; set; } = string.Empty;

        // ==========================================================
        // System Audit Fields
        // ==========================================================
        // Basic Properties
        public long ModifiedTransactionId { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(DimensionAttributeValueSet))]
//         public virtual DimensionAttributeValueSet? AttributeValueSet { get; set; }

//         [ForeignKey(nameof(DimensionAttributeValue))]
//         public virtual DimensionAttributeValue? AttributeValue { get; set; }

        #endregion
    }
}

