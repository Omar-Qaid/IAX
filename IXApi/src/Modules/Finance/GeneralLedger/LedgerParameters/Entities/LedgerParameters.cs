using IAX.IXApi.Modules.Finance.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    /// <summary>
    /// General ledger parameters: one record per company, identified by Key_.
    ///
    /// This is the basic core of the D365 table. Its country and region fields (_CN, _RU, _SA,
    /// _IT, _MX, _TH, _ES, _DK, _GB, _EG, _IN, _JP), its public sector fields (_PSN) and the
    /// advanced feature groups — budget encumbrance, revenue recognition, cash control,
    /// withholding tax, electronic reporting format mappings and batch tuning — are deliberately
    /// not carried here. They belong with the features that read them.
    /// </summary>
    [Table("LedgerParameters")]
    public class LedgerParameters : Entity<long>
    {
        //----------------------------------------- Record Identity
        // Basic Properties
        public int Key_ { get; set; } // Single parameter record per company

        // ==========================================================
        // Voucher & Journal Controls
        // ==========================================================
        // Basic Properties
        public long AbortErrorAccount { get; set; } // Ledger dimension used when posting must report an error account

        // Enum Properties
        public NoYes CheckContinuous { get; set; }
        public NoYes CheckVoucher { get; set; }
        public NoYes CheckJournalizing { get; set; }
        public NoYes AllowMultipleTransactionsWithinOneVoucher { get; set; }
        public int DimensionValuesUsedForSummaryAccount { get; set; }

        // ==========================================================
        // Reversal Presentation
        // ==========================================================
        // Enum Properties
        public NoYes ReverseOppositeSignAmounts { get; set; }
        public NoYes TransactionReversalCorrection { get; set; }

        // ==========================================================
        // Rounding Tolerance
        // ==========================================================
        // Basic Properties
        public decimal MaxRoundingDifferenceMst { get; set; }
        public decimal MaxRoundingDifferenceMstSecond { get; set; }

        // ==========================================================
        // Period & Year End
        // ==========================================================
        // Enum Properties
        public NoYes LimitedOpenFiscalPeriods { get; set; }
        public NoYes PeriodClosingPosting { get; set; }
        public NoYes DeleteOpening { get; set; }
        public NoYes OpeningVoucher { get; set; }
        public NoYes OpeningWithoutBudget { get; set; }
        public NoYes YearClosed { get; set; }

        // ==========================================================
        // Ledger Settlement
        // ==========================================================
        // Enum Properties
        public NoYes EnableLedgerSettlement { get; set; }
        public NoYes AdvancedLedgerSettlement { get; set; }
        public NoYes EnableLedgerSettlementPartialSettlement { get; set; }
        public NoYes EnableLedgerSettlementPostRealizedGainLoss { get; set; }
        public NoYes EnableLedgerSettlementSubledgerSettlement { get; set; }
        public NoYes EnableLedgerSettlementAdvancedAwarenessOptions { get; set; }
        public NoYes EnableLedgerSettlementProcessAutomation { get; set; }
    }
}
