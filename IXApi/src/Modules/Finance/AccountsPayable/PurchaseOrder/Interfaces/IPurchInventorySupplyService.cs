namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IPurchInventorySupplyService
{
    /// <summary>Creates the ordered inventory receipt for a new purchase line (no physical stock).</summary>
    Task CreateAsync(PurchTable order, PurchLine line, string inventSiteId, string inventLocationId,
        CancellationToken ct = default, string? inventBatchId = null, string? inventSerialId = null);

    /// <summary>
    /// Releases the ordered supply a purchase line is still expecting, leaving everything already
    /// received untouched. Returns the inventory quantity cancelled.
    /// <para>
    /// <paramref name="inventQty"/> releases only that part of the remainder, which is how a line's
    /// open quantity is reduced rather than cancelled outright; omitting it releases the whole
    /// remainder.
    /// </para>
    /// </summary>
    Task<decimal> CancelRemainingAsync(PurchTable order, PurchLine line,
        decimal? inventQty = null, CancellationToken ct = default);
}
