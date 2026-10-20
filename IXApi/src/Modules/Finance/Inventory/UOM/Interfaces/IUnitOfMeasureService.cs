namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IUnitOfMeasureService
{
    Task<List<UnitOfMeasureDto>> ListAsync(CancellationToken ct = default);
    Task<(UnitOfMeasureDto? Unit, string? Error)> CreateAsync(UnitOfMeasureInputDto input, CancellationToken ct = default);
    Task<(UnitOfMeasureDto? Unit, string? Error)> UpdateAsync(long recId, UnitOfMeasureInputDto input, CancellationToken ct = default);
    Task<string?> DeleteAsync(long recId, CancellationToken ct = default);
    Task<(UnitOfMeasureConversionDto? Conversion, string? Error)> SaveConversionAsync(long? recId, UnitOfMeasureConversionInputDto input, CancellationToken ct = default);
    Task<string?> DeleteConversionAsync(long recId, CancellationToken ct = default);
}
