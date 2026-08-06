using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("LogisticsAddressState")]
    public class LogisticsAddressState : Entity<long>
    {
        //----------------------------------------- Core Identity & Regional Naming
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.StateId)]
        public string StateId { get; set; } = string.Empty; // Primary alphanumeric code key (e.g., CA, TX, BY)

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty; // Official descriptive name of the state or province

        // ==========================================================
        // Geopolitical Parent Relationships & Defaults
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CountryRegionId)]
        public string CountryRegionId { get; set; } = string.Empty; // Parent code anchor link to LogisticsAddressCountryRegion

        // Enum Properties
        public NoYes DefaultStateForCountryRegion { get; set; } // Fallback flag used during address resolution if no state is provided

        // ==========================================================
        // Chronological / Regional Demarcations
        // ==========================================================
        // Basic Properties
        public int TimeZone { get; set; } // Regional timezone override indicator for the specific state boundary

  
        #region Navigation Properties Row
        //LogisticsAddressState.CountryRegionId == LogisticsAddressCountryRegion.CountryRegionId
        [ForeignKey(nameof(CountryRegionId))]
        public virtual LogisticsAddressCountryRegion? LogisticsAddressCountryRegionTable { get; set; }

        #endregion
       
    }
}
