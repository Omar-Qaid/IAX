using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed record CustStatementDto(
    string AccountNum,
    DateTime FromDate,
    DateTime ToDate,
    decimal OpeningBalanceMst,
    decimal ClosingBalanceMst,
    IReadOnlyList<CustStatementLineDto> Lines);

public sealed record CustStatementLineDto(
    long RecId,
    DateTime TransDate,
    string Invoice,
    string Voucher,
    LedgerTransType TransType,
    string CurrencyCode,
    decimal AmountCur,
    decimal AmountMst,
    decimal RunningBalanceMst);
