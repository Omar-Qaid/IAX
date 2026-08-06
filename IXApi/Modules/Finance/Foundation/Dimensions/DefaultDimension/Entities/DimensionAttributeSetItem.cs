using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("DimensionAttributeSetItem")]
    public class DimensionAttributeSetItem : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        public long DimensionAttributeSet { get; set; }
        public long DimensionAttribute { get; set; }
        public int EnumerationValue { get; set; } // Represents the sequence/position index within the set


        #region Navigation Properties Row

//         [ForeignKey(nameof(DimensionAttributeSet))]
//         public virtual DimensionAttributeSet? AttributeSet { get; set; }

//         [ForeignKey(nameof(DimensionAttribute))]
//         public virtual DimensionAttribute? AttributeDefinition { get; set; }

        #endregion
    }
}

