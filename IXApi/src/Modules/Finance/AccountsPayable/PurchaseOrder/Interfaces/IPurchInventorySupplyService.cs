namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IPurchInventorySupplyService
{
    /// <summary>Creates the ordered inventory receipt for a new purchase line (no physical stock).</summary>
    Task CreateAsync(PurchTable order, PurchLine line, string inventSiteId, string inventLocationId, CancellationToken ct = default);
}
