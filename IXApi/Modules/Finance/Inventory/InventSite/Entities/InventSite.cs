using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("InventSite")]
    public class InventSite : Entity<long>
    {
        //----------------------------------------- Core Information & Identity
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.InventSiteId)]
        public string SiteId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        // ==========================================================
        // Logistical Framework & Defaults
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.DefaultInventStatusID)]
        public string DefaultInventStatusID { get; set; } = string.Empty;

        // Enum Properties
        public Timezone TimeZone { get; set; }
        public NoYes IsReceivingWarehouseOverrideAllowed { get; set; }

        // ==========================================================
        // Tax & Financial Integration
        // ==========================================================
        // Basic Properties
        public long TaxBranchRefRecId { get; set; } // Regulatory/Localization Tax Branch Anchor
        public long DefaultDimension { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(DefaultDimension))]
//         public virtual DimensionAttributeValueSet? DefaultDimensionSet { get; set; }

        #endregion
    }
}

