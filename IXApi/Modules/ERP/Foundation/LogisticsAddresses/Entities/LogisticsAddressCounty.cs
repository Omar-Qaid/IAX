using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("LogisticsAddressCounty")]
    public class LogisticsAddressCounty : Entity<long>
    {
        //----------------------------------------- Core Identity & Structural Mappings
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CountyId)]
        public string CountyId { get; set; } = string.Empty; // Primary alphanumeric key code for the county (e.g., Cook, Orange)

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty; // Official naming convention for the county/sub-district boundary

        // ==========================================================
        // Geopolitical Hierarchical Relationships
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CountryRegionId)]
        public string CountryRegionId { get; set; } = string.Empty; // Code validation anchor pointing to LogisticsAddressCountryRegion

        [Required]
        [StringLength(FieldLengths.StateId)]
        public string StateId { get; set; } = string.Empty; // Structural state boundary code link pointing to LogisticsAddressState


        #region Navigation Properties Row
        //LogisticsAddressCounty.CountryRegionId == LogisticsAddressCountryRegion.CountryRegionId
        [ForeignKey(nameof(CountryRegionId))]
        public virtual LogisticsAddressCountryRegion? LogisticsAddressCountryRegionTable { get; set; }

        //LogisticsAddressCounty.CountryRegionId == LogisticsAddressState.CountryRegionId
        //LogisticsAddressCounty.StateId == LogisticsAddressState.StateId
        [ForeignKey(nameof(StateId))]
        public virtual LogisticsAddressState? LogisticsAddressStateTable { get; set; }

        #endregion

    }
}
