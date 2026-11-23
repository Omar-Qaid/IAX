namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;

/// <summary>The <c>Seeding</c> configuration section, also bound from <c>DatabaseInitialization</c>.</summary>
public sealed class SeedingOptions
{
    public const string SectionName = "Seeding";
    /// <summary>Runs the one-time imports from the configured OthersDB database.</summary>
    public bool EnableLegacyImports { get; set; }
    /// <summary>Lets the one-time imports fall back to the embedded JSON when no OthersDB connection is configured.</summary>
    public bool EnableEmbeddedFixtures { get; set; }
    public int LockTimeoutSeconds { get; set; } = 120;
    public string DefaultCompany { get; set; } = "dat";
}
