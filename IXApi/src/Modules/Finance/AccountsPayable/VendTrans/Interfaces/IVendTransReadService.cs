namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IVendTransReadService
{
    Task<VendTransDto?> GetAsync(long recId, CancellationToken ct);
    Task<VendTransPageDto> SearchAsync(VendTransQueryDto query, CancellationToken ct);

    /// <summary>
    /// Reads the open vendor balance from VendTransOpen, for one vendor or for all of them.
    /// </summary>
    /// <param name="accountNum">The vendor, or null for every vendor in the company.</param>
    /// <param name="asOfDate">Excludes transactions dated after this date; defaults to today.</param>
    Task<VendOpenBalanceDto> GetOpenBalanceAsync(string? accountNum, DateTime? asOfDate, CancellationToken ct);
}
