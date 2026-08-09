using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("LogisticsElectronicAddress")]
    public class LogisticsElectronicAddress : Entity<long>
    {
        //----------------------------------------- Core Identity & Structural Anchors
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ElectronicAddressId)]
        public string ElectronicAddressId { get; set; } = string.Empty;

        public long Location { get; set; } // Foreign Key reference pointing to LogisticsLocation master record

        [Required]
        [StringLength(FieldLengths.Description)]
        public string Description { get; set; } = string.Empty; // Descriptive user label (e.g., "Main Corporate Email")

        // Enum Properties
        public ElectronicAddressType Type { get; set; } // e.g., Phone, Email, URL, Telex, Fax, Instant Message

        // ==========================================================
        // Telephony, Digital Addressing & Content Fields
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Locator)]
        public string Locator { get; set; } = string.Empty; // Stores actual phone number strings, email strings, or URLs

        [Required]
        [StringLength(FieldLengths.LocatorExtension)]
        public string LocatorExtension { get; set; } = string.Empty; // Holds secondary extension fragments (e.g., Ext 404)

        // ==========================================================
        // Functional Roles & Prioritization Flags
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ElectronicAddressRoles)]
        public string ElectronicAddressRoles { get; set; } = string.Empty; // Semi-colon separated flattened string collection of customs roles

        // Enum Properties
        public NoYes IsPrimary { get; set; }      // Default fall-back target for the designated address type group
        public NoYes IsMobilePhone { get; set; }  // Optimization flag identifying SMS/Text capabilities
        public NoYes IsInstantMessage { get; set; }

        // ==========================================================
        // Security Controls & Private Party Data Protections
        // ==========================================================
        // Basic Properties
        public long? PrivateForParty { get; set; } // Pointer restricting access strictly to a particular DirPartyTable entity record

        // Enum Properties
        public NoYes IsPrivate { get; set; } // Hides data visibility from broad cross-functional directory users

        // ==========================================================
        // Commerce Channel / Retail Operations Integrations
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ChannelReferenceId)]
        public string ChannelReferenceId { get; set; } = string.Empty; // Synced UUID or reference ID from external e-commerce engines

        // Enum Properties
        public NoYes RetailMarketingOptIn { get; set; } // Strategic GDPR compliant digital outbound marketing filter policy


        #region Navigation Properties Row
        //LogisticsElectronicAddress.Location == LogisticsLocation.RecId
        [ForeignKey(nameof(Location))]
        public virtual LogisticsLocation? LogisticsLocationTable { get; set; }


        //LogisticsElectronicAddress.PrivateForParty == DirPartyTable.RecId
        [ForeignKey(nameof(PrivateForParty))]
        public virtual DirPartyTable? DirPartyTable { get; set; }

        #endregion
   
    }
}

