using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public interface ICustTransService : IBaseService<CustTrans>
{
    /// <summary>
    /// Reads the open customer balance from CustTransOpen, for one customer or for all of them.
    /// </summary>
    /// <param name="accountNum">The customer, or null for every customer in the company.</param>
    /// <param name="asOfDate">Excludes transactions dated after this date; defaults to today.</param>
    Task<CustOpenBalanceDto> GetOpenBalanceAsync(string? accountNum, DateTime? asOfDate, CancellationToken ct);
}
