namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Organization;

/// <summary>Stable business references described by the Organization manifest and checked against persisted seeders.</summary>
public static class OrganizationSeedExample
{
    public static IReadOnlyList<(string Entity, string Field, string Value)> References { get; } =
    [
        ("DocuType","TypeId","File"),
        ("DocuType","TypeId","Note"),
        ("DocuType","TypeId","URL"),
        ("DocuType","TypeId","Image")
    ];
}
