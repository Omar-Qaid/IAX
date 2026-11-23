namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Organization;

/// <summary>The tables this module owns, as covered by its registered seeders.</summary>
public static class OrganizationTableCatalog
{
    public static IReadOnlySet<string> MasterTypes { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "DocuType",
    };

    /// <summary>
    /// Attachments are written by the document service, which owns the stored file, its hash and the
    /// owning record reference. Nothing here seeds them.
    /// </summary>
    public static IReadOnlySet<string> TransactionTypes { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "DocuRef",
        "DocuValue",
    };
}
