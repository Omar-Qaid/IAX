using DocumentFormat.OpenXml.Spreadsheet;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Shared.Features;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("LogisticsAddressCountryRegion")]
    public class LogisticsAddressCountryRegion : Entity<long>
    {
        //----------------------------------------- Core Identity & Geopolitical Standards
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CountryRegionId)]
        public string CountryRegionId { get; set; } = string.Empty; // Primary alphanumeric key identifier (e.g., USA, DEU)

        [Required]
        [StringLength(FieldLengths.IsoCode)]
        public string IsoCode { get; set; } = string.Empty; // Standard ISO-3166 2-letter country code (e.g., US, DE)

        // ==========================================================
        // Structural Address Architecture & Validation Controls
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.AddrFormat)]
        public string AddrFormat { get; set; } = string.Empty; // Layout routing string mapping postal address presentation fields

        // Enum Properties
        public NoYes AddressUseZipPlus4 { get; set; } // Flag activating extended US ZIP+4 postal format checking parameters

        // ==========================================================
        // Chronological & Governance Directives
        // ==========================================================
        // Basic Properties
        public int TimeZone { get; set; } // Default baseline timezone enumeration lookup ID for the destination zone

        // Enum Properties
        public NoYes IsImmutable { get; set; } // Lock indicator preventing user deletion or disruption of system-critical regions



        #region Navigation Properties Row
        //LogisticsAddressCountryRegion.RPayParentCountryRegionId == LogisticsAddressCountryRegion.CountryRegionId
  

        #endregion
    }
}
