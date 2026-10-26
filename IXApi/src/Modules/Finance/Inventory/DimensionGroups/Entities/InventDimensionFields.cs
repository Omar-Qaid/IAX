namespace IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;

// D365 stores table field IDs (fieldNum(InventDim, ...)) in the *FldSetup tables.
// These IDs are environment metadata, so callers resolve them through this single registry.
public static class InventDimensionFields
{
    public const int Site = 1;
    public const int Warehouse = 2;
    public const int Location = 3;
    public const int BatchNumber = 4;
    public const int SerialNumber = 5;
}
