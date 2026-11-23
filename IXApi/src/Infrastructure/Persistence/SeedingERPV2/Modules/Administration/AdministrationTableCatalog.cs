namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Administration;

/// <summary>The tables this module owns, as covered by its registered seeders.</summary>
public static class AdministrationTableCatalog
{
    public static IReadOnlySet<string> MasterTypes { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "SysBackgroundJob",
        "SysNumberSequence",
        "SysSettings",
    };

    /// <summary>
    /// Run history and allocated codes are written by the batch runner and the number sequence
    /// service as they execute. Nothing here seeds them.
    /// </summary>
    public static IReadOnlySet<string> TransactionTypes { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "SysBackgroundJobExecution",
    };
}
