using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("DimensionAttributeValueGroupCombination")]
    public class DimensionAttributeValueGroupCombination : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        public long DimensionAttributeValueCombination { get; set; }
        public long DimensionAttributeValueGroup { get; set; }
        public int Ordinal { get; set; }

        // ==========================================================
        // System Audit Fields
        // ==========================================================
        // Basic Properties
        public long ModifiedTransactionId { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(DimensionAttributeValueCombination))]
//         public virtual DimensionAttributeValueCombination? ValueCombination { get; set; }

//         [ForeignKey(nameof(DimensionAttributeValueGroup))]
//         public virtual DimensionAttributeValueGroup? ValueGroup { get; set; }

        #endregion
    }
}

