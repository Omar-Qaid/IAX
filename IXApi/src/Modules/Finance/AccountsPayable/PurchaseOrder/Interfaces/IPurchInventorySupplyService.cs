namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IPurchInventorySupplyService
{
    /// <summary>Creates the ordered inventory receipt for a new purchase line (no physical stock).</summary>
    Task CreateAsync(PurchTable order, PurchLine line, string inventSiteId, string inventLocationId, CancellationToken ct = default);

    /// <summary>
    /// Releases the ordered supply a purchase line is still expecting, leaving everything already
    /// received untouched. Returns the inventory quantity cancelled.
    /// </summary>
    Task<decimal> CancelRemainingAsync(PurchTable order, PurchLine line, CancellationToken ct = default);
}
