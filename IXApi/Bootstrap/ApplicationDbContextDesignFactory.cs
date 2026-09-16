using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IAX.IXApi.Bootstrap;

/// <summary>Scaffold without starting the API, running seeders, or loading runtime secrets.</summary>
public sealed class ApplicationDbContextDesignFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connection = args.Contains("--schema-only", StringComparer.Ordinal)
            ? "Server=localhost;Database=IXApiSchemaOnly;Integrated Security=true;TrustServerCertificate=true"
            : Environment.GetEnvironmentVariable("IXAPI_MIGRATIONS_CONNECTION")
                ?? throw new InvalidOperationException("Set IXAPI_MIGRATIONS_CONNECTION for database operations, or pass --schema-only for offline scaffolding/script generation.");
        return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connection).Options);
    }
}
