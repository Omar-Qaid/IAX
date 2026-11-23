using System.Text;
using System.Text.Json;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;

/// <summary>
/// Where a one-time legacy import reads from: the OthersDB database named by <c>ConnectionStrings:SeedDbConnString</c>,
/// or, when none is configured and <c>Seeding:EnableEmbeddedFixtures</c> allows it, the sanitized export embedded as
/// <c>Modules/{Module}/Data/legacy-*.json</c>. Credentials and personal contact fields are never part of either.
/// </summary>
public sealed class LegacyImportSource(IConfiguration configuration, IOptions<SeedingOptions> options)
{
    public const string ConnectionName = "SeedDbConnString";
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Runs <paramref name="sql"/> (a <c>FOR JSON</c> query) on OthersDB, or reads the embedded <paramref name="file"/>.</summary>
    public async Task<T> ReadAsync<T>(string sql, string file, CancellationToken ct)
    {
        var connection = configuration.GetConnectionString(ConnectionName);
        if (string.IsNullOrWhiteSpace(connection))
        {
            if (!options.Value.EnableEmbeddedFixtures)
                throw new InvalidOperationException(
                    $"Enable Seeding:EnableEmbeddedFixtures or configure ConnectionStrings:{ConnectionName} before running the legacy import.");
            await using var stream = typeof(LegacyImportSource).Assembly.GetManifestResourceStream("SeedingERPV2.Import." + file)
                ?? throw new InvalidOperationException("Missing embedded legacy import: " + file);
            return await JsonSerializer.DeserializeAsync<T>(stream, Json, ct)
                ?? throw new InvalidOperationException("Empty embedded legacy import: " + file);
        }

        await using var sqlConnection = new SqlConnection(connection);
        await sqlConnection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, sqlConnection) { CommandTimeout = 120 };
        await using var reader = await command.ExecuteReaderAsync(ct);
        var text = new StringBuilder();
        while (await reader.ReadAsync(ct))
            if (!reader.IsDBNull(0)) text.Append(reader.GetString(0));
        return JsonSerializer.Deserialize<T>(text.ToString(), Json)
            ?? throw new InvalidOperationException($"{ConnectionName} returned no data for {file}.");
    }
}
