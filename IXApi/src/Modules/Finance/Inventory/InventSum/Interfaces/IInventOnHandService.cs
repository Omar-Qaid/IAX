namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventOnHandService
{
    Task<List<InventOnHandDto>?> ListAsync(CancellationToken ct = default);
}
