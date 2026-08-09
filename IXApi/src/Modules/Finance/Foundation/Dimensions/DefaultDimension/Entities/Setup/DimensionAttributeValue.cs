using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("DimensionAttributeValue")]
    public class DimensionAttributeValue : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        public long DimensionAttribute { get; set; }
        public long EntityInstance { get; set; }
        public long OriginalEntityInstance { get; set; }

        [Required]
        [StringLength(FieldLengths.DisplayValue)]
        public string DisplayValue { get; set; } = string.Empty;

        public Guid HashKey { get; set; }

        // ==========================================================
        // Validity & Lifecycle States
        // ==========================================================
        // Basic Properties
        public DateTime ActiveFrom { get; set; }
        public DateTime ActiveTo { get; set; }

        // Enum Properties
        public NoYes IsBlockedForManualEntry { get; set; }
        public NoYes IsSuspended { get; set; }
        public NoYes IsTotal { get; set; }
        public new NoYes IsDeleted { get; set; }
        public NoYes PendingSuccessfulDeleteValidation { get; set; }

        // ==========================================================
        // Multi-Company & Classification Contexts
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.BackingRecordDataAreaId)]
        public string BackingRecordDataAreaId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.GroupDimension)]
        public string GroupDimension { get; set; } = string.Empty;

        public long Owner { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(DimensionAttribute))]
//         public virtual DimensionAttribute? DimensionAttributeTable { get; set; }

        #endregion
    }
}


