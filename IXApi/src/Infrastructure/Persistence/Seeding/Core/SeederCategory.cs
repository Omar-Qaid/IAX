namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Core
{
    /// <summary>
    /// Categorization for seeders governing inclusion rules and execution safety.
    /// </summary>
    public enum SeederCategory
    {
        /// <summary>
        /// Critical system seeders essential for application operation (e.g. Identity, Settings).
        /// </summary>
        Required = 1,

        /// <summary>
        /// Standard reference / master data required for business functionality (e.g. Currencies, Countries, UOM).
        /// </summary>
        Reference = 2,

        /// <summary>
        /// Optional demo, sample, or training data. Excluded by default unless explicitly enabled.
        /// </summary>
        Demo = 3,

        /// <summary>
        /// External database legacy migration or snapshot import. Excluded by default unless explicitly enabled.
        /// </summary>
        LegacyImport = 4
    }
}
