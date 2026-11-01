using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>One open vendor transaction, as VendTransOpen holds it.</summary>
public sealed record VendOpenTransDto(
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
/// A vendor's open balance: the sum of its open transactions, with the detail behind it. Payable
/// amounts are credits, so an open balance owed to a vendor is negative.
/// </summary>
/// <param name="Reconciled">
/// False when any open row disagrees with its transaction — that is, when
/// VendTransOpen.AmountCur is not VendTrans.AmountCur less VendTrans.SettleAmountCur. The payment
/// path refuses to settle in that state, so the read reports it rather than presenting a balance
/// that settlement would reject.
/// </param>
public sealed record VendOpenBalanceDto(
    string? AccountNum,
    DateTime AsOfDate,
    decimal OpenAmountCur,
    decimal OpenAmountMst,
    int TransactionCount,
    bool Reconciled,
    List<VendOpenTransDto> Transactions);
