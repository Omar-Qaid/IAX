using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Shared.Features;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("LogisticsAddressZipCode")]
    public class LogisticsAddressZipCode : Entity<long>
    {
        //----------------------------------------- Core Identity & Primary Index Coordinates
        // Basic Properties
        [Required]
        [StringLength(10)]
        public string ZipCode { get; set; } = string.Empty; // Primary postal code lookup string (e.g., "90210", "SW1A 1AA")

        [Required]
        [StringLength(10)]
        public string CountryRegionId { get; set; } = string.Empty; // Direct geopolitical parent link code

        // ==========================================================
        // Geographical Hierarchy Backstops (Fallbacks)
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(30)]
        public string State { get; set; } = string.Empty; // Default state code used during postal auto-completion

        [Required]
        [StringLength(30)]
        public string County { get; set; } = string.Empty; // Default county code used during postal auto-completion

        [Required]
        [StringLength(60)]
        public string City { get; set; } = string.Empty; // Primary default city name string

        [Required]
        [StringLength(30)]
        public string CityAlias { get; set; } = string.Empty; // Alternative, localized, or abbreviated municipal alias

        [Required]
        [StringLength(60)]
        public string DistrictName { get; set; } = string.Empty;

        // ==========================================================
        // Explicit Normalized Relational Hierarchy Links
        // ==========================================================
        // Basic Properties
        public long CityRecId { get; set; } // Direct structural verification link to LogisticsAddressCity
        public long District { get; set; }  // Direct structural verification link to LogisticsAddressDistrict

        // ==========================================================
        // Street Range Specific Validation Rules (Post-Offices Delivery Grid)
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(250)]
        public string StreetName { get; set; } = string.Empty; // Associated street name if this ZIP code is isolated to a specific road

        public int FromNum { get; set; } // Lower boundary numerical threshold for address street ranges
        public int ToNum { get; set; }   // Upper boundary numerical threshold for address street ranges

        // Enum Properties
        public EvenOdd EvenOdd { get; set; } // Directs validation constraints matching only Even, Odd, or All building numbers

        // ==========================================================
        // Chronological & Regional Parameters
        // ==========================================================
        // Basic Properties
        public int TimeZone { get; set; } // Regional timezone override indicator for this specific postal sector

        /*
        #region Navigation Properties

        // LogisticsAddressZipCode.CountryRegionId == LogisticsAddressCountryRegion.CountryRegionId
        [ForeignKey(nameof(CountryRegionId))]
        public virtual LogisticsAddressCountryRegion? LogisticsAddressCountryRegionTable { get; set; }

        // LogisticsAddressZipCode.CityRecId == LogisticsAddressCity.RecId
        [ForeignKey(nameof(CityRecId))]
        public virtual LogisticsAddressCity? LogisticsAddressCityByRecId { get; set; }

        // LogisticsAddressZipCode.City == LogisticsAddressCity.Name
        [ForeignKey(nameof(City))]
        public virtual LogisticsAddressCity? LogisticsAddressCityByName { get; set; }

        // LogisticsAddressZipCode.District == LogisticsAddressDistrict.RecId
        [ForeignKey(nameof(District))]
        public virtual LogisticsAddressDistrict? LogisticsAddressDistrictByRecId { get; set; }

        // LogisticsAddressZipCode.District == LogisticsAddressDistrict.RecId
        // LogisticsAddressZipCode.DistrictName == LogisticsAddressDistrict.Name
        public virtual LogisticsAddressDistrict? LogisticsAddressDistrictByName { get; set; }

        // LogisticsAddressZipCode.State == LogisticsAddressState.StateId
        // LogisticsAddressZipCode.CountryRegionId == LogisticsAddressState.CountryRegionId
        public virtual LogisticsAddressState? LogisticsAddressStateNavigation { get; set; }

        // LogisticsAddressZipCode.County == LogisticsAddressCounty.CountyId
        // LogisticsAddressZipCode.State == LogisticsAddressCounty.StateId
        // LogisticsAddressZipCode.CountryRegionId == LogisticsAddressCounty.CountryRegionId
        public virtual LogisticsAddressCounty? LogisticsAddressCountyNavigation { get; set; }

        #endregion
          */
    }
}
