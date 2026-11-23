using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Tests.Support.Persistence;

/// <summary>
/// A private in-memory SQLite database holding the full application schema with foreign keys enforced. It lives
/// as long as this object: every context made from <see cref="Options"/> shares the same connection, and disposing
/// drops the database, so each test starts empty and leaves nothing behind.
/// </summary>
public sealed class SqliteTestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    private SqliteTestDatabase(SqliteConnection connection, ICompanyExecutionContext? companyContext)
    {
        _connection = connection;
        Options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
        Context = CreateContext(companyContext);
    }

    public DbContextOptions<ApplicationDbContext> Options { get; }

    /// <summary>The open connection, for raw SQL checks that must bypass the context.</summary>
    public SqliteConnection Connection => _connection;

    /// <summary>The context the test works with.</summary>
    public SqliteApplicationDbContext Context { get; }

    /// <summary>Opens the database and, unless the test times schema creation itself, creates the schema.</summary>
    public static async Task<SqliteTestDatabase> CreateAsync(ICompanyExecutionContext? companyContext = null, bool createSchema = true)
    {
        var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        var database = new SqliteTestDatabase(connection, companyContext);
        if (createSchema) await database.CreateSchemaAsync();
        return database;
    }

    /// <summary>A further context on the same database, e.g. to read back without the first context's tracked state.</summary>
    public SqliteApplicationDbContext CreateContext(ICompanyExecutionContext? companyContext = null) => new(Options, companyContext);

    /// <summary>
    /// Creates the schema from the production model script, so constraints run exactly as on SQL Server; only SQL
    /// Server Unicode literals are normalized for SQLite.
    /// </summary>
    public Task CreateSchemaAsync() =>
        Context.Database.ExecuteSqlRawAsync(Context.Database.GenerateCreateScript().Replace("N'", "'"));

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
