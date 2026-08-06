using IAX.IXApi.Modules.ERP.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.ERP.Entities
{    /*
     SELECT
    ct.AccountNum,
    dp.Name,
    lpa.Street,
    lpa.StreetNumber,
    lpa.City,
    lpa.County,
    lpa.ZipCode,
    lpa.State,
    lpa.CountryRegionId,
    lpa.ValidFrom,
    lpa.ValidTo
FROM CustTable ct
INNER JOIN DirPartyTable dp
    ON dp.RecId = ct.Party
INNER JOIN DirPartyLocation dpl
    ON dpl.Party = dp.RecId
INNER JOIN LogisticsPostalAddress lpa
    ON lpa.Location = dpl.Location
WHERE
    dpl.IsPostalAddress = 1
    AND lpa.ValidTo >= GETUTCDATE()
ORDER BY ct.AccountNum;



SELECT
    ct.AccountNum,
    dp.Name,
    lea.Type,
    lea.Locator,
    lea.IsPrimary,
    lea.Description
FROM CustTable ct
INNER JOIN DirPartyTable dp
    ON dp.RecId = ct.Party
INNER JOIN DirPartyLocation dpl
    ON dpl.Party = dp.RecId
INNER JOIN LogisticsElectronicAddress lea
    ON lea.Location = dpl.Location
ORDER BY ct.AccountNum, lea.Type;
     */
    [Table("LogisticsPostalAddress")]
    public class LogisticsPostalAddress : Entity<long>
    {
        //----------------------------------------- Core Identity & Framework Anchors
        // Basic Properties
        public long Location { get; set; } // Foreign Key reference pointing to the master LogisticsLocation record

        [Required]
        [StringLength(FieldLengths.Address)]
        public string Address { get; set; } = string.Empty; // Full consolidated, systemformatted address string block

        // ==========================================================
        // Chronological Date-Effectivity Matrix (Valid Time State)
        // ==========================================================
        // Basic Properties
        public DateTime ValidFrom { get; set; }
        public int ValidFromTzId { get; set; } // Timezone normalization lookup ID for the state start delta

        public DateTime ValidTo { get; set; }
        public int ValidToTzId { get; set; } // Timezone normalization lookup ID for the state termination delta

        // ==========================================================
        // Granular Postal Address Segments
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Street)]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.StreetNumber)]
        public string StreetNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.BuildingCompliment)]
        public string BuildingCompliment { get; set; } = string.Empty; // Multi-tenant details (e.g., Suite 400, Floor 3)

        [Required]
        [StringLength(FieldLengths.PostBox)]
        public string PostBox { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.City)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.County)]
        public string County { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.State)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ZipCode)]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.CountryRegionId)]
        public string CountryRegionId { get; set; } = string.Empty; // Maps to standard LogisticsAddressCountryRegion

        [Required]
        [StringLength(FieldLengths.DistrictName)]
        public string DistrictName { get; set; } = string.Empty;

        // ==========================================================
        // System Master Relational Record ID Pointers
        // ==========================================================
        // Basic Properties
        public long CityRecId { get; set; }       // Linked key pointing back to LogisticsAddressCity setup
        public long ZipCodeRecId { get; set; }    // Linked key pointing back to LogisticsAddressZipCode setup
        public long District { get; set; }        // Structured record ID reference link to the district master
        public long LocalityRecId { get; set; }    // Structured record ID reference link to the localized zone master
        public long SettlementRecId { get; set; } // Direct structural connection to regional tax collection jurisdictions

        // ==========================================================
        // Spatial Geolocation, Coordinates & Environment
        // ==========================================================
        // Basic Properties
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int TimeZone { get; set; } // Local timezone classification ID for delivery/logistics processing

        // ==========================================================
        // Security Controls & Private Party Scope Restrictions
        // ==========================================================
        // Basic Properties
        public long? PrivateForParty { get; set; } // Reference lock restriction pointing to a specific DirPartyTable record

        // Enum Properties
        public NoYes IsPrivate { get; set; } // Access-layer security mask hiding address from generic inquiries

        // ==========================================================
        // Commerce Channel Sync & Omni-Channel Primitives
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ChannelReferenceId)]
        public string ChannelReferenceId { get; set; } = string.Empty; // Integrated UUID mapping token for POS/Web channels



        #region Navigation Properties

        // LogisticsPostalAddress.Location == LogisticsLocation.RecId
        [ForeignKey(nameof(Location))]
        public virtual LogisticsLocation? LogisticsLocationTable { get; set; }

        // LogisticsPostalAddress.PrivateForParty == DirPartyTable.RecId
        [ForeignKey(nameof(PrivateForParty))]
        public virtual DirPartyTable? DirPartyTable { get; set; }

        // LogisticsPostalAddress.CountryRegionId == LogisticsAddressCountryRegion.CountryRegionId
        [ForeignKey(nameof(CountryRegionId))]
        public virtual LogisticsAddressCountryRegion? LogisticsAddressCountryRegionTable { get; set; }

        // LogisticsPostalAddress.CityRecId == LogisticsAddressCity.RecId
        [ForeignKey(nameof(CityRecId))]
        public virtual LogisticsAddressCity? LogisticsAddressCityByRecId { get; set; }

        // LogisticsPostalAddress.City == LogisticsAddressCity.Name
        [ForeignKey(nameof(City))]
        public virtual LogisticsAddressCity? LogisticsAddressCityByName { get; set; }

        // LogisticsPostalAddress.State == LogisticsAddressState.StateId
        // LogisticsPostalAddress.CountryRegionId == LogisticsAddressState.CountryRegionId
        public virtual LogisticsAddressState? LogisticsAddressStateTable { get; set; }

        // LogisticsPostalAddress.County == LogisticsAddressCounty.CountyId
        // LogisticsPostalAddress.State == LogisticsAddressCounty.StateId
        // LogisticsPostalAddress.CountryRegionId == LogisticsAddressCounty.CountryRegionId
        public virtual LogisticsAddressCounty? LogisticsAddressCountyTable { get; set; }

        // LogisticsPostalAddress.District == LogisticsAddressDistrict.RecId
        [ForeignKey(nameof(District))]
        public virtual LogisticsAddressDistrict? LogisticsAddressDistrictByRecId { get; set; }

        // LogisticsPostalAddress.DistrictName == LogisticsAddressDistrict.Name
        [ForeignKey(nameof(DistrictName))]
        public virtual LogisticsAddressDistrict? LogisticsAddressDistrictByName { get; set; }


        // LogisticsPostalAddress.ZipCodeRecId == LogisticsAddressZipCode.RecId
        [ForeignKey(nameof(ZipCodeRecId))]
        public virtual LogisticsAddressZipCode? LogisticsAddressZipCodeByRecId { get; set; }

        // LogisticsPostalAddress.ZipCode == LogisticsAddressZipCode.ZipCode
        [ForeignKey(nameof(ZipCode))]
        public virtual LogisticsAddressZipCode? LogisticsAddressZipCodeTable { get; set; }
        #endregion

    }
}
