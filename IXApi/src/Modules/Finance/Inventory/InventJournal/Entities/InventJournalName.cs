using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("InventJournalName")]
    public class InventJournalName : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.JournalNameId)]
        public string JournalNameId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Description)]
        public string Description { get; set; } = string.Empty;

        // Enum Properties
        public InventJournalType JournalType { get; set; }

        // ==========================================================
        // Voucher & Number Sequence Management
        // ==========================================================
        // Basic Properties
        public long VoucherNumberSequenceTable { get; set; }

        // Enum Properties
        public JournalVoucherChange VoucherChange { get; set; }
        public JournalVoucherDraw VoucherDraw { get; set; }

        // ==========================================================
        // Financial Integration & Approvals
        // ==========================================================
        // Basic Properties
        public long LedgerDimension { get; set; }

        // Enum Properties
        public NoYes WorkflowApproval { get; set; }

        // ==========================================================
        // Posting, Operations & Lifecycle Rules
        // ==========================================================
        // Enum Properties
        public DetailSummary DetailSummary { get; set; }
        public NoYes DeletePostedLines { get; set; }
        public ItemReservation Reservation { get; set; }
        public CountingStatusRegistrationPolicy CountingStatusRegistrationPolicy { get; set; }
        public NoYes ExcludeWarehouseInventoryUpdateLogs { get; set; }

        // ==========================================================
        // Commerce / Retail Adjustments
        // ==========================================================
        // Enum Properties
        public RetailInventJournalPosAdjustmentType RetailInventJournalPosAdjustmentType { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(LedgerDimension))]
//         public virtual DimensionAttributeValueCombination? DefaultLedgerAccount { get; set; }

        #endregion
    }
}

