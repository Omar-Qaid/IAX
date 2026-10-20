namespace IAX.IXApi.Modules.Finance.Inventory;
public interface IInventParametersService
{
    Task<InventParametersDto?> GetAsync(CancellationToken ct = default);
    Task<InventParametersLookupsDto?> GetLookupsAsync(CancellationToken ct = default);
    Task<(InventParametersDto? Value, string? Error)> SaveAsync(InventParametersDto input, CancellationToken ct = default);
}
