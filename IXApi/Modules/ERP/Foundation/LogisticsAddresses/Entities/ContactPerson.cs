using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("ContactPerson")]
    public class ContactPerson : Entity<long>
    {
        //----------------------------------------- Core Identity & Global Directory Links
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ContactPersonId)]
        public string ContactPersonId { get; set; } = string.Empty;

        public long Party { get; set; }           // Direct reference link pointing to the DirPartyTable record of this person
        public long ContactForParty { get; set; }  // Reference link pointing to the parent company/organization's DirPartyTable record

        [Required]
        [StringLength(FieldLengths.CustAccount)]
        public string CustAccount { get; set; } = string.Empty; // Direct cross-reference shortcut to CustTable if linked to a Customer Account

        // ==========================================================
        // Life Cycle Status & Operational Governance
        // ==========================================================
        // Enum Properties
        public NoYes Inactive { get; set; }
        public NoYes Vip { get; set; }
        public NoYes Imported { get; set; } // Identifies if the record was originated via data migration packages/external APIs
        public NoYes IsContactPersonExternallyMaintained { get; set; } // Flag routing read-only locks if managed by external master hub systems
        public ContactSensitivity Sensitivity { get; set; } // Public, Private, Confidential classifications

        // ==========================================================
        // Internal Personnel Assignment & Availability
        // ==========================================================
        // Basic Properties
        public long MainResponsibleWorker { get; set; } // Reference pointer to HcmWorker representing the internal account manager

        public int TimeAvailableFrom { get; set; } // Daily operational window start time represented in seconds from midnight
        public int TimeAvailableTo { get; set; }   // Daily operational window end time represented in seconds from midnight

        // ==========================================================
        // CRM Marketing & Communication Restrictions
        // ==========================================================
        // Enum Properties
        public NoYes DirectMail { get; set; } // Opt-in/Opt-out policy marker for outbound physical or digital marketing campaigns
        public NoYes McrIsDefaultContact { get; set; } // Call center extension flag evaluating default contact hierarchy

        // ==========================================================
        // Multi-Channel External Portal Access & Roles
        // ==========================================================
        // Enum Properties
        public NoYes VendorPortalAccessAllowed { get; set; } // Grants authentication clearance for external vendor collaboration workspaces
        public NoYes WebRequestAccess { get; set; }          // Grants customer self-service e-commerce request clearance
        public VendorContactRole VendRole { get; set; }       // Strategic categorization of vendor contact function (e.g., Sales, Escalations)

        // ==========================================================
        // System Concurrency Audit Stamps
        // ==========================================================
        // Basic Properties
        public DateTime LastEditAxDateTime { get; set; }
        public int LastEditAxDateTimeTzId { get; set; } // Timezone enumeration context ID for accurate chronological delta audits


        #region Navigation Properties Row
        //ContactPerson.Party == DirPartyTable.RecId
        [ForeignKey(nameof(Party))]
        public virtual DirPartyTable? DirPartyTable { get; set; }

        #endregion
    }
}
