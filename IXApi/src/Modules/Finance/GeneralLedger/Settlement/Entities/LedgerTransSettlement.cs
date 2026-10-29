using IAX.IXApi.Modules.Finance.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    /// <summary>
    /// Ledger settlements: marks posted ledger account entries as matched against each other.
    /// SettleId groups the entries settled together and SettlementGroup links the group's rows.
    /// Settlement records matching only; in the equal same-currency case it changes no balance.
    /// </summary>
    [Table("LedgerTransSettlement")]
    public class LedgerTransSettlement : Entity<long>
    {
        //----------------------------------------- Settlement Identity & Settled Transaction
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Voucher)]
        public string SettleId { get; set; } = string.Empty; // Settlement ID grouping the matched entries

        public long TransRecId { get; set; }    // Settled GeneralJournalAccountEntry
        public long RefTransRecId { get; set; } // Counterpart transaction reference
        public long SettlementGroup { get; set; }
        public int SettlementTableId { get; set; } // Table id of the settled transaction

        [Required]
        [StringLength(FieldLengths.Voucher)]
        public string OriginalSettleId { get; set; } = string.Empty;

        // ==========================================================
        // Settlement Chronology & Processing Context
        // ==========================================================
        // Basic Properties
        public DateTime SettleDate { get; set; }    // Date settled
        public DateTime DateProcessed { get; set; } // Date processed

        [Required]
        [StringLength(FieldLengths.Voucher)]
        public string Voucher { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.PostedUserId)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Voucher)]
        public string AutomationRule { get; set; } = string.Empty;

        // ==========================================================
        // Settled Amounts & Exchange Adjustment
        // ==========================================================
        // Basic Properties
        public decimal SettleTransactionCurrencyAmount { get; set; }
        public decimal SettleAccountingCurrencyAmount { get; set; }
        public decimal SettleReportingCurrencyAmount { get; set; }
        public decimal ExchAdjAccountingCurrencyAmount { get; set; }
        public decimal ExchAdjReportingCurrencyAmount { get; set; }

        // ==========================================================
        // Reversal, Reason & System State
        // ==========================================================
        // Basic Properties
        public long Reason { get; set; } // Reason reference
        public int SysDataStateCode { get; set; }

        // Enum Properties
        public NoYes IsReversed { get; set; }
    }
}
