namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// One aging bucket. The bounds are days overdue against the transaction's due date, inclusive;
/// a null lower bound is everything not yet due and a null upper bound is everything older than
/// the last named period.
/// </summary>
public sealed record VendAgingBucketDto(
    int? MinimumDaysOverdue,
    int? MaximumDaysOverdue,
    decimal OpenAmountCur,
    decimal OpenAmountMst,
    int TransactionCount);

/// <summary>One vendor's open balance split into the same buckets as the report that holds it.</summary>
public sealed record VendAgingRowDto(
    string AccountNum,
    decimal OpenAmountCur,
    decimal OpenAmountMst,
    int TransactionCount,
    IReadOnlyList<VendAgingBucketDto> Buckets);

/// <summary>
/// A vendor aging report: the open balance of one vendor or of every vendor, split by how overdue
/// each open transaction is. Payable amounts are credits, so a balance owed to a vendor is
/// negative, exactly as <see cref="VendOpenBalanceDto"/> reports it.
/// </summary>
/// <param name="OpenAmountCur">
/// Summed in each transaction's own currency, so it is meaningful only for a single-currency
/// selection; <paramref name="OpenAmountMst"/> is the accounting-currency total.
/// </param>
/// <param name="Reconciled">
/// False when any open row disagrees with its transaction, carried through from the open balance
/// read so an aging report cannot present a balance settlement would reject.
/// </param>
public sealed record VendAgingDto(
    string? AccountNum,
    DateTime AsOfDate,
    decimal OpenAmountCur,
    decimal OpenAmountMst,
    int TransactionCount,
    bool Reconciled,
    IReadOnlyList<VendAgingBucketDto> Buckets,
    IReadOnlyList<VendAgingRowDto> Vendors);
