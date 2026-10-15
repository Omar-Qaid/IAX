namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventPostingRuleDto
{
    public string Id { get; init; } = string.Empty;
    public int InventAccountType { get; init; }
    public int ItemCode { get; init; }
    public string ItemRelation { get; init; } = string.Empty;
    public string CategoryRelation { get; init; } = "0";
    public int CustVendCode { get; init; }
    public string CustVendRelation { get; init; } = string.Empty;
    public string TaxGroupId { get; init; } = string.Empty;
    public string LedgerDimension { get; init; } = "0";
    public string MainAccount { get; init; } = string.Empty;
}
