using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests.Support.Persistence;

/// <summary>
/// A disposable database on the opted-in local SQL Server (<see cref="TestEnvironment.SqlServer"/>). The name is
/// always generated from the prefix and a new GUID, so a test can never reach an application database; disposing
/// drops it. Use only from a test gated by <see cref="SqlServerFactAttribute"/>.
/// </summary>
public sealed class SqlServerTestDatabase : IAsyncDisposable
{
    private readonly string _prefix;

    private SqlServerTestDatabase(string prefix, int? commandTimeoutSeconds)
    {
        _prefix = prefix;
        Name = prefix + Guid.NewGuid().ToString("N");
        ConnectionString = TestEnvironment.SqlServerConnectionString(Name);
        Options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString, sql => { if (commandTimeoutSeconds is { } seconds) sql.CommandTimeout(seconds); })
            .Options;
    }

    public string Name { get; }
    public string ConnectionString { get; }
    public DbContextOptions<ApplicationDbContext> Options { get; }

    /// <summary>Names a new database; nothing is created until a context creates the schema.</summary>
    public static SqlServerTestDatabase Create(string prefix, int? commandTimeoutSeconds = null) => new(prefix, commandTimeoutSeconds);

    public ApplicationDbContext CreateContext(ICompanyExecutionContext? companyContext = null) => new(Options, companyContext: companyContext);

    public async ValueTask DisposeAsync()
    {
        Assert.StartsWith(_prefix, Name);
        await using var cleanup = CreateContext();
        await cleanup.Database.EnsureDeletedAsync();
    }
}
