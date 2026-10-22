namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventItemBarcodeInputDto(string ItemBarcode, string UnitId, decimal Qty,
    string Description = "", string InventDimId = "", bool UseForInput = true,
    bool UseForPrinting = false, bool Blocked = false);

public sealed record InventItemBarcodeDto(long RecId, string ItemBarcode, string ItemId,
    string UnitId, decimal Qty, string Description, string InventDimId,
    bool UseForInput, bool UseForPrinting, bool Blocked);

public sealed record InventItemBarcodeLookupDto(long ReleasedProductRecId, InventItemBarcodeDto Barcode);

