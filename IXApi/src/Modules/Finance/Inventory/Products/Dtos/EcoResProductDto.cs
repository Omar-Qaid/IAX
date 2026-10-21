namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class EcoResProductDto
{
    public string Id { get; init; } = string.Empty;
    public string ProductNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string NameAlias { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string SearchName { get; init; } = string.Empty;
    public string ProductType { get; init; } = string.Empty;
    public string ProductSubtype { get; init; } = string.Empty;
}

public sealed class EcoResProductInputDto
{
    public string ProductNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string NameAlias { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string SearchName { get; init; } = string.Empty;
    public string ProductType { get; init; } = "Item";
}
