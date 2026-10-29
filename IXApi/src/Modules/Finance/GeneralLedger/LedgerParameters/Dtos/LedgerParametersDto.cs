using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerParameters;

/// <summary>General ledger parameters of the selected company.</summary>
public sealed record LedgerParametersDto(
    long AbortErrorAccount,
    NoYes CheckContinuous,
    NoYes CheckVoucher,
    NoYes CheckJournalizing,
    NoYes AllowMultipleTransactionsWithinOneVoucher,
    NoYes ReverseOppositeSignAmounts,
    NoYes TransactionReversalCorrection,
    decimal MaxRoundingDifferenceMst,
    NoYes LimitedOpenFiscalPeriods,
    NoYes PeriodClosingPosting,
    NoYes DeleteOpening,
    NoYes OpeningVoucher,
    NoYes YearClosed,
    NoYes EnableLedgerSettlement,
    NoYes EnableLedgerSettlementPartialSettlement);
