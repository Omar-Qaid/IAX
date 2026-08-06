using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("LogisticsAddressCity")]
    public class LogisticsAddressCity : Entity<long>
    {
        //----------------------------------------- Core Identity & Structural Anchors
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CityKey)]
        public string CityKey { get; set; } = string.Empty; // Unique human-readable system composite index key

        public long CityRecId { get; set; } // Self-referencing structural validation fallback pointer

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty; // Absolute official naming convention for the city node

        [Required]
        [StringLength(FieldLengths.Description)]
        public string Description { get; set; } = string.Empty; // Alternative description or localized language tag

        // ==========================================================
        // Regional Context Hierarchical Relationships
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CountryRegionId)]
        public string CountryRegionId { get; set; } = string.Empty; // Direct strategic code link to LogisticsAddressCountryRegion

        [Required]
        [StringLength(FieldLengths.StateId)]
        public string StateId { get; set; } = string.Empty; // Strategic code boundary link to LogisticsAddressState

        [Required]
        [StringLength(FieldLengths.CountyId)]
        public string CountyId { get; set; } = string.Empty; // Strategic sub-district/county boundary code link if utilized

        // ==========================================================
        // Municipal, Administrative & Localized Allocations
        // ==========================================================
        // Basic Properties
        public long SettlementRecId { get; set; } // Direct link pointing to municipal, township, or localized tax jurisdictions


   

        #region Navigation Properties Row

        // LogisticsAddressCity.CountryRegionId == LogisticsAddressCountryRegion.CountryRegionId
        [ForeignKey(nameof(CountryRegionId))]
        public virtual LogisticsAddressCountryRegion? LogisticsAddressCountryRegionTable { get; set; }

        // LogisticsAddressCity.CountryRegionId == LogisticsAddressState.CountryRegionId
        // LogisticsAddressCity.StateId == LogisticsAddressState.StateId
        public virtual LogisticsAddressState? LogisticsAddressStateTable { get; set; }

        // LogisticsAddressCity.CountryRegionId == LogisticsAddressCounty.CountryRegionId
        // LogisticsAddressCity.StateId == LogisticsAddressCounty.StateId
        // LogisticsAddressCity.CountyId == LogisticsAddressCounty.CountyId
        public virtual LogisticsAddressCounty? LogisticsAddressCountyTable { get; set; }

        #endregion
  
    }
}

