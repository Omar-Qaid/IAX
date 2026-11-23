namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;

/// <summary>When a baseline seeder runs.</summary>
public enum BaselineSeederKind
{
    /// <summary>Runs on every startup and only adds what is missing.</summary>
    Repeatable,
    /// <summary>A one-time import of legacy data, gated by <c>Seeding:EnableLegacyImports</c>.</summary>
    LegacyImport
}

/// <summary>
/// The startup profile. Registered only as <see cref="IBaselineSeeder"/>, never as a plain <see cref="IDataSeeder"/>,
/// so the explicit example runs never pick up a baseline seeder and startup never runs an example.
/// </summary>
public interface IBaselineSeeder : IDataSeeder
{
    BaselineSeederKind Kind => BaselineSeederKind.Repeatable;
    int Version => 1;
    /// <summary>History scope. For an import it must stay the scope earlier runs logged.</summary>
    string Scope => "dat";
}
