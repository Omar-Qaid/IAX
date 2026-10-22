namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IInventItemBarcodeService
{
    Task<(List<InventItemBarcodeDto>? Rows, string? Error)> ListAsync(long releasedProductRecId, CancellationToken ct);
    Task<(List<InventItemBarcodeLookupDto>? Rows, string? Error)> FindAsync(string barcode, CancellationToken ct);
    Task<(InventItemBarcodeDto? Row, string? Error)> CreateAsync(long releasedProductRecId, InventItemBarcodeInputDto input, CancellationToken ct);
    Task<(InventItemBarcodeDto? Row, string? Error)> UpdateAsync(long releasedProductRecId, long barcodeRecId, InventItemBarcodeInputDto input, CancellationToken ct);
    Task<(bool Deleted, string? Error)> DeleteAsync(long releasedProductRecId, long barcodeRecId, CancellationToken ct);
}
