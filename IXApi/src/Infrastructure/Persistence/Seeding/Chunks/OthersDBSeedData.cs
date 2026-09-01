using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Text;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>
/// Base class for seeders that read sanitized data from the database configured
/// by <c>ConnectionStrings:SeedDbConnString</c>.
/// </summary>
public abstract class OthersDBSeedData : ISeeder
{
    protected OthersDBSeedData(string? seedDbConnectionString)
    {
        SeedDbConnectionString = string.IsNullOrWhiteSpace(seedDbConnectionString)
            ? null
            : seedDbConnectionString;
    }

    protected string? SeedDbConnectionString { get; }

    protected static async Task<string> ReadJsonAsync(
        string connectionString,
        string sql,
        CancellationToken ct)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(sql, connection)
        {
            CommandTimeout = 120,
        };
        await using var reader = await command.ExecuteReaderAsync(ct);
        var json = new StringBuilder();
        while (await reader.ReadAsync(ct))
        {
            if (!reader.IsDBNull(0))
            {
                json.Append(reader.GetString(0));
            }
        }

        return json.ToString();
    }

    public abstract Task SeedAsync(
        ApplicationDbContext db,
        RoleManager<AspNetRole> roles,
        UserManager<AspNetUser> users,
        CancellationToken ct);
}
