using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("DirPartyLocation")]
    public class DirPartyLocation : Entity<long>
    {
        //----------------------------------------- Core Identity & Link Matrix Mapping
        // Basic Properties
        public long Party { get; set; }    // Foreign Key Reference to the global entity record (DirPartyTable RecId)
        public long Location { get; set; } // Foreign Key Reference to the generic destination record (LogisticsLocation RecId)

        // ==========================================================
        // Strategic Routing Rules & Address Priority
        // ==========================================================
        // Enum Properties
        public NoYes IsPrimary { get; set; }                // The absolute default location link for this party record
        public NoYes IsPostalAddress { get; set; }           // Distinguishes hard mailing addresses from electronic contact points
        public NoYes IsPrivate { get; set; }                 // Imposes granular security filter visibility rules
        public NoYes IsLocationOwner { get; set; }           // Indicates legal title/ownership of the location structure
        public NoYes IsPrimaryTaxRegistration { get; set; }  // Signals to the taxation engine that this is the primary fiscal node

        // ==========================================================
        // Functional Roles Realization (Boolean Fast-Path Accessors)
        // ==========================================================
        // Enum Properties
        public NoYes IsRoleBusiness { get; set; } // Flagged if location is an active workspace or headquarters
        public NoYes IsRoleDelivery { get; set; } // Flagged to shortcut supply-chain shipment target evaluations
        public NoYes IsRoleInvoice { get; set; }  // Flagged to shortcut accounts payable/receivable mailing lines
        public NoYes IsRoleHome { get; set; }     // Flagged if location maps to a residential context

        // ==========================================================
        // Combined Strategic Classifications
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.PostalAddressRoles)]
        public string PostalAddressRoles { get; set; } = string.Empty; // Semi-colon flattened list of secondary custom roles

        // ==========================================================
        // Chronological Assignment Audit Trails
        // ==========================================================
        // Basic Properties
        public DateTime AssignmentDate { get; set; }
        public int AssignmentDateTzId { get; set; } // Timezone normalization lookup identifier


        #region Navigation Properties Row
        //DirPartyLocation.Party == DirPartyTable.RecId
        [ForeignKey(nameof(Party))]
        public virtual DirPartyTable? DirPartyTable { get; set; }

        //DirPartyLocation.Location == LogisticsLocation.RecId
        [ForeignKey(nameof(Location))]
        public virtual LogisticsLocation? LogisticsLocationTable { get; set; }

        #endregion
    }
}
