namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Core
{
    /// <summary>
    /// Configuration options for the seeding subsystem.
    /// </summary>
    public class SeedingOptions
    {
        public const string SectionName = "Seeding";

        /// <summary>
        /// Global switch to enable database seeding.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// When true, demo and sample data seeders (SeederCategory.Demo) will be executed.
        /// </summary>
        public bool EnableDemoData { get; set; } = false;

        /// <summary>
        /// When true, legacy database imports (SeederCategory.LegacyImport) will be executed.
        /// </summary>
        public bool EnableLegacyImports { get; set; } = false;

        /// <summary>
        /// When true, embedded snapshot fixtures can be used as fallback when external connection is absent.
        /// </summary>
        public bool EnableEmbeddedFixtures { get; set; } = false;

        /// <summary>
        /// Concurrency lock acquisition timeout in seconds.
        /// </summary>
        public int LockTimeoutSeconds { get; set; } = 120;

        /// <summary>
        /// Default company scope when seeder does not specify one.
        /// </summary>
        public string DefaultCompany { get; set; } = "dat";
    }
}
