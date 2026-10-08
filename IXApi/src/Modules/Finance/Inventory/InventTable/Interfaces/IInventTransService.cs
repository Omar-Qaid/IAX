namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventTransService
{
    Task<List<InventTransListDto>?> GetListAsync(CancellationToken cancellationToken = default);
}
