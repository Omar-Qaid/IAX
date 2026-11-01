using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

/// <summary>One open customer transaction, as CustTransOpen holds it.</summary>
public sealed record CustOpenTransDto(
    long RecId,
    string Invoice,
    string Voucher,
    LedgerTransType TransType,
    DateTime TransDate,
    DateTime DueDate,
    string CurrencyCode,
    decimal AmountCur,
    decimal SettleAmountCur,
    decimal OpenAmountCur,
    decimal OpenAmountMst,
    int DaysOverdue);

/// <summary>
/// A customer's open balance: the sum of its open transactions, with the detail behind it.
/// </summary>
/// <param name="Reconciled">
/// False when any open row disagrees with its transaction — that is, when
/// CustTransOpen.AmountCur is not CustTrans.AmountCur less CustTrans.SettleAmountCur. The payment
/// path refuses to settle in that state, so the read reports it rather than presenting a balance
/// that settlement would reject.
/// </param>
public sealed record CustOpenBalanceDto(
    string? AccountNum,
    DateTime AsOfDate,
    decimal OpenAmountCur,
    decimal OpenAmountMst,
    int TransactionCount,
    bool Reconciled,
    List<CustOpenTransDto> Transactions);
