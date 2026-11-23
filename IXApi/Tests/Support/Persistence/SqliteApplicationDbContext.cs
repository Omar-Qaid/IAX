using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Tests.Support.Persistence;

/// <summary>
/// The production <see cref="ApplicationDbContext"/> model, with every relationship, key and index kept, and only
/// the SQL Server storage types and default-value SQL normalized so the schema can be created in SQLite.
/// </summary>
public sealed class SqliteApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICompanyExecutionContext? companyContext = null)
    : ApplicationDbContext(options, companyContext: companyContext)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        foreach (var entity in builder.Model.GetEntityTypes())
        foreach (var property in entity.GetProperties())
        {
            var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
            property.SetColumnType(type == typeof(byte[]) ? "BLOB"
                : type == typeof(string) || type == typeof(DateTime) || type == typeof(decimal) || type == typeof(Guid) || type == typeof(DateOnly)
                    ? "TEXT" : "INTEGER");
            property.SetDefaultValueSql(null);
            if (property.Name == "RowVersion") property.SetDefaultValueSql("X'00'");
        }
    }
}
