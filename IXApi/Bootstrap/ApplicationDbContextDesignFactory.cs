using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IAX.IXApi.Bootstrap;

/// <summary>Scaffold without starting the API, running seeders, or loading runtime secrets.</summary>
public sealed class ApplicationDbContextDesignFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuredConnection = BuildConfiguration().GetConnectionString("DbConnString");
        var connection = args.Contains("--schema-only", StringComparer.Ordinal)
            ? "Server=localhost;Database=IXApiSchemaOnly;Integrated Security=true;TrustServerCertificate=true"
            : Environment.GetEnvironmentVariable("IXAPI_MIGRATIONS_CONNECTION")
                ?? (string.IsNullOrWhiteSpace(configuredConnection)
                    ? throw new InvalidOperationException("Set IXAPI_MIGRATIONS_CONNECTION or ConnectionStrings:DbConnString for database operations, or pass --schema-only for offline scaffolding/script generation.")
                    : configuredConnection);
        return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connection).Options);
    }

    private static IConfiguration BuildConfiguration() => new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
        .AddUserSecrets<ApplicationDbContextDesignFactory>(optional: true)
        .AddEnvironmentVariables()
        .Build();
}
