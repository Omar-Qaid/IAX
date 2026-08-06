using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("InventJournalTable")]
    public class InventJournalTable : Entity<long>
    {
        //----------------------------------------- Core Identity & Relationship Links
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.JournalId)]
        public string JournalId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.JournalNameId)]
        public string JournalNameId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Description)]
        public string Description { get; set; } = string.Empty;

        public int NumOfLines { get; set; }

        [Required]
        [StringLength(FieldLengths.Source)]
        public string Source { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.JournalIdOrignal)]
        public string JournalIdOrignal { get; set; } = string.Empty; // Original journal tracer map context

        // Enum Properties
        public InventJournalType JournalType { get; set; }
        public InventJournalOriginType JournalOriginType { get; set; }

        // ==========================================================
        // Logistical & Inventory Dimensions Scopes
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.InventSiteId)]
        public string InventSiteId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.InventLocationId)]
        public string InventLocationId { get; set; } = string.Empty;

        public int InventDimFixed { get; set; } // Bitmask pattern specifying fixed default segments

        // ==========================================================
        // Voucher & Number Sequence Controls
        // ==========================================================
        // Basic Properties
        public long VoucherNumberSequenceTable { get; set; }

        // Enum Properties
        public JournalVoucherChange VoucherChange { get; set; }
        public JournalVoucherDraw VoucherDraw { get; set; }

        // ==========================================================
        // Financial Integration & Workflow Approvals
        // ==========================================================
        // Basic Properties
        public long LedgerDimension { get; set; }

        // Enum Properties
        public WorkflowApprovalStatus WorkflowApprovalStatus { get; set; }

        // ==========================================================
        // Posting History & Audit Trail Attributes
        // ==========================================================
        // Basic Properties
        public DateTime PostedDateTime { get; set; }
        public int PostedDateTimeTzId { get; set; }

        [Required]
        [StringLength(FieldLengths.PostedUserId)]
        public string PostedUserId { get; set; } = string.Empty;

        public long Worker { get; set; }

        // Enum Properties
        public NoYes Posted { get; set; }

        // ==========================================================
        // Operational Parameters & Registration Policies
        // ==========================================================
        // Enum Properties
        public DetailSummary DetailSummary { get; set; }
        public NoYes DeletePostedLines { get; set; }
        public ItemReservation Reservation { get; set; }
        public CountingStatusRegistrationPolicy CountingStatusRegistrationPolicy { get; set; }
        public int InventoryServiceJournalExpectedStatus { get; set; }

        // ==========================================================
        // Commerce / Retail Multi-Channel Operations
        // ==========================================================
        // Enum Properties
        public NoYes IsRetailCommitted { get; set; }
        public int RetailReplenishmentType { get; set; }
        public int RetailRetailStatusType { get; set; }

        // ==========================================================
        // Active Concurrency, Sessions & System Locks
        // ==========================================================
        // Basic Properties
        public int SessionId { get; set; }
        public DateTime SessionLoginDateTime { get; set; }
        public int SessionLoginDateTimeTzId { get; set; }
        public int SysDataStateCode { get; set; }

        // Enum Properties
        public NoYes SystemBlocked { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(JournalNameId))]
//         public virtual InventJournalName? JournalName { get; set; }

//         [ForeignKey(nameof(LedgerDimension))]
//         public virtual DimensionAttributeValueCombination? DefaultLedgerAccount { get; set; }

//         [ForeignKey(nameof(InventSiteId))]
//         public virtual InventSite? InventSite { get; set; }

//         [ForeignKey(nameof(InventLocationId))]
//         public virtual InventLocation? InventLocation { get; set; }

//         [ForeignKey(nameof(Worker))]
//         public virtual IAX.IXApi.Modules.Organization.Employees.OrgEmployee? Employee { get; set; }

        #endregion
    }
}
