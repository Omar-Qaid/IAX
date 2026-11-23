using Microsoft.Data.SqlClient;
using Xunit;

namespace IAX.IXApi.Tests.Support;

/// <summary>
/// Opt-in switches for tests that need a local SQL Server. Without them those tests are skipped, so the default
/// run needs nothing but SQLite.
/// </summary>
public static class TestEnvironment
{
    /// <summary>SQL Server instance for disposable test databases (integrated security).</summary>
    public const string SqlServerVariable = "IXAPI_AP_PHASE_ONE_SQLSERVER";
    /// <summary>Set to <c>1</c> to also run the all-module SeedingERPV2 replay on SQL Server.</summary>
    public const string SeedSqlServerVariable = "IAX_SEED_ERP_V2_SQL_TESTS";
    /// <summary>Set to <c>1</c> to run the batch session-lock tests against SQL Server <c>master</c>.</summary>
    public const string BatchSqlServerVariable = "IAX_BATCH_SQL_TESTS";

    public static string? SqlServer => Environment.GetEnvironmentVariable(SqlServerVariable);

    public static string SqlServerConnectionString(string database) => new SqlConnectionStringBuilder
    {
        DataSource = string.IsNullOrWhiteSpace(SqlServer) ? "." : SqlServer,
        InitialCatalog = database, IntegratedSecurity = true, TrustServerCertificate = true, ConnectTimeout = 30
    }.ConnectionString;

    internal static bool IsSet(string variable) => Environment.GetEnvironmentVariable(variable) == "1";
}

/// <summary>Runs only when <see cref="TestEnvironment.SqlServerVariable"/> names a local SQL Server.</summary>
public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(TestEnvironment.SqlServer))
            Skip = $"Set {TestEnvironment.SqlServerVariable} to opt into a disposable local SQL Server database.";
    }
}

/// <summary>Runs only when <see cref="TestEnvironment.SeedSqlServerVariable"/> is <c>1</c>.</summary>
public sealed class SqlServerSeedFactAttribute : FactAttribute
{
    public SqlServerSeedFactAttribute()
    {
        if (!TestEnvironment.IsSet(TestEnvironment.SeedSqlServerVariable))
            Skip = $"Set {TestEnvironment.SeedSqlServerVariable}=1 to create an isolated database on local SQL Server.";
    }
}

/// <summary>Runs only when <see cref="TestEnvironment.BatchSqlServerVariable"/> is <c>1</c>.</summary>
public sealed class SqlServerBatchFactAttribute : FactAttribute
{
    public SqlServerBatchFactAttribute()
    {
        if (!TestEnvironment.IsSet(TestEnvironment.BatchSqlServerVariable))
            Skip = $"Opt in to SQL Server session-lock tests with {TestEnvironment.BatchSqlServerVariable}=1.";
    }
}
