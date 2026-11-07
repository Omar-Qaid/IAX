namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed record CustAgingDto(
    string AccountNum,
    DateTime BalanceAsOf,
    DateTime AgingAsOf,
    decimal BalanceMst,
    IReadOnlyList<CustAgingBucketDto> Buckets);

public sealed record CustAgingBucketDto(int? MinimumDaysOverdue, int? MaximumDaysOverdue, decimal BalanceMst);
