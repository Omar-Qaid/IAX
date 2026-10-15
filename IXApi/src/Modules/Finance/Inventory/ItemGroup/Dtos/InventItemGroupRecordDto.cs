namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventItemGroupPostingDto
{
    public string Id { get; init; } = string.Empty;
    public int AccountType { get; init; }
    public string LedgerDimension { get; init; } = "0";
    public string MainAccount { get; init; } = string.Empty;
}

public sealed class InventItemGroupRecordDto
{
    public string Id { get; init; } = string.Empty;
    public string ItemGroupId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public List<InventItemGroupPostingDto> Postings { get; init; } = [];
}
