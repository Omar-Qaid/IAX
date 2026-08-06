using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("GeneralJournalEntry")]
    public class GeneralJournalEntry : Entity<long>
    {
        //----------------------------------------- Core Information & Identity
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.JournalNumber)]
        public string JournalNumber { get; set; } = string.Empty;

        public long Ledger { get; set; }
        public long LedgerEntryJournal { get; set; }

        // Enum Properties
        public LedgerJournalCategory JournalCategory { get; set; }
        public CurrentToOperationsPostingLayer PostingLayer { get; set; }

        // ==========================================================
        // Vouchers, Documents & Sub-Ledger Linking
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.SubledgerVoucher)]
        public string SubledgerVoucher { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.SubledgerVoucherDataAreaId)]
        public string SubledgerVoucherDataAreaId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.DocumentNumber)]
        public string DocumentNumber { get; set; } = string.Empty;

        public DateTime AccountingDate { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime AcknowledgementDate { get; set; }

        // ==========================================================
        // Fiscal Calendar References
        // ==========================================================
        // Basic Properties
        public long FiscalCalendarPeriod { get; set; }
        public long FiscalCalendarYear { get; set; }

        // ==========================================================
        // Budgeting, Transfers & System Auditing
        // ==========================================================
        // Basic Properties
        public long BudgetSourceLedgerEntryPosted { get; set; }
        public long TransferId { get; set; }
        public long CreatedTransactionId { get; set; }
        public int SysDataStateCode { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(FiscalCalendarPeriod))]
//         public virtual FiscalCalendarPeriod? CalendarPeriod { get; set; }

//         [ForeignKey(nameof(FiscalCalendarYear))]
//         public virtual FiscalCalendarYear? CalendarYear { get; set; }

//         [ForeignKey(nameof(Ledger))]
//         public virtual Ledger? LedgerDefinition { get; set; }

        #endregion
    }
}
