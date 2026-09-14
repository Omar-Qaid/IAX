using System.Data;
using System.Data.Common;
using IAX.IXApi.Modules.Administration.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services;

/// <summary>SQL Server session-owned lock on a dedicated connection, released on disposal/crash.</summary>
public sealed class BatchDatabaseLock : IAsyncDisposable
{
    private readonly DbConnection connection;
    private BatchDatabaseLock(DbConnection connection) => this.connection = connection;

    public static async Task<BatchDatabaseLock?> TryAcquireAsync(IAdministrationDataContext db,
        string resource, CancellationToken ct)
    {
        var template = db.Database.GetDbConnection();
        var connection = DbProviderFactories.GetFactory(template).CreateConnection()
            ?? throw new InvalidOperationException("The database provider cannot create a lock connection.");
        connection.ConnectionString = db.Database.GetConnectionString() ?? template.ConnectionString;
        try
        {
            await connection.OpenAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = "DECLARE @result int; EXEC @result = sys.sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 0; SELECT @result;";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@resource"; parameter.Value = resource;
            command.Parameters.Add(parameter);
            var result = Convert.ToInt32(await command.ExecuteScalarAsync(ct));
            if (result >= 0) return new BatchDatabaseLock(connection) { resourceKey = resource };
            await connection.DisposeAsync();
            return null;
        }
        catch { await connection.DisposeAsync(); throw; }
    }

    public async Task CheckAsync(CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT APPLOCK_MODE('public', @key, 'Session')";
        var parameter = command.CreateParameter(); parameter.ParameterName = "@key"; parameter.Value = resourceKey;
        command.Parameters.Add(parameter);
        if (!string.Equals(Convert.ToString(await command.ExecuteScalarAsync(ct)), "Exclusive", StringComparison.Ordinal))
            throw new InvalidOperationException("The batch database lock was lost.");
    }

    public async ValueTask DisposeAsync()
    {
        // Session locks must be released explicitly before returning a pooled connection.
        if (connection.State == ConnectionState.Open)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "DECLARE @resource nvarchar(255) = @key; EXEC sys.sp_releaseapplock @Resource = @resource, @LockOwner = 'Session';";
            // All lock instances own exactly one resource.
            var parameter = command.CreateParameter(); parameter.ParameterName = "@key"; parameter.Value = resourceKey;
            command.Parameters.Add(parameter);
            try { await command.ExecuteNonQueryAsync(); }
            finally { await connection.DisposeAsync(); }
        }
        else await connection.DisposeAsync();
    }
    private string resourceKey = string.Empty;
}
