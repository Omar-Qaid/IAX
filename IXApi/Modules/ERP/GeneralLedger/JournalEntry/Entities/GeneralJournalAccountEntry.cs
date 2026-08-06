using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Modules.ERP.GeneralLedger;
using IAX.IXApi.Modules.ERP.Shared.Features;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("GeneralJournalAccountEntry")]
    public class GeneralJournalAccountEntry : Entity<long>
    {
        //----------------------------------------- Core Information & Ledger Dimensions
        // Basic Properties
        public long GeneralJournalEntry { get; set; }
        public long LedgerDimension { get; set; }
        public long MainAccount { get; set; }

        [Required]
        [StringLength(FieldLengths.LedgerAccount)]
        public string LedgerAccount { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Text)]
        public string Text { get; set; } = string.Empty;

        // Enum Properties
        public LedgerPostingType PostingType { get; set; }

        // ==========================================================
        // Financial Amounts & Multi-Currency Values
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.TransactionCurrencyCode)]
        public string TransactionCurrencyCode { get; set; } = string.Empty;

        public decimal TransactionCurrencyAmount { get; set; }
        public decimal AccountingCurrencyAmount { get; set; }
        public decimal ReportingCurrencyAmount { get; set; }
        public decimal Quantity { get; set; }
        public DateTime HistoricalExchangeRateDate { get; set; }

        // Enum Properties
        public NoYes IsCredit { get; set; }
        public NoYes IsCorrection { get; set; }

        // ==========================================================
        // Operational References & Sub-Ledger Extensions
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.PaymentReference)]
        public string PaymentReference { get; set; } = string.Empty;

        public long FinTag { get; set; } // Financial Tag Reference
        public long ReasonRef { get; set; }
        public long OriginalAccountEntry { get; set; }

        // Enum Properties
        public int AllocationLevel { get; set; } // Map to LedgerAllocationLevel if preferred
        public int AssetLeasePostingTypes { get; set; }
        public int AssetLeaseTransactionType { get; set; }

        // ==========================================================
        // System Audit & Data State Controls
        // ==========================================================
        // Basic Properties
        public long CreatedTransactionId { get; set; }
        public int SysDataStateCode { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(GeneralJournalEntry))]
//         public virtual GeneralJournalEntry? JournalEntry { get; set; }

//         [ForeignKey(nameof(LedgerDimension))]
//         public virtual DimensionAttributeValueCombination? AccountCombination { get; set; }

//         [ForeignKey(nameof(MainAccount))]
//         public virtual MainAccount? MainAccountDefinition { get; set; }

//         [ForeignKey(nameof(TransactionCurrencyCode))]
//         public virtual Currency? TransactionCurrency { get; set; }

//         [ForeignKey(nameof(OriginalAccountEntry))]
//         public virtual GeneralJournalAccountEntry? OriginalEntry { get; set; }

        #endregion
    }
}
