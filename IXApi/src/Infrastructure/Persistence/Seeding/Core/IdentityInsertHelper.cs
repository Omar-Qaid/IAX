using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Core
{
    public interface IIdentityInsertService
    {
        Task ExecuteWithIdentityInsertAsync(
            DbContext db,
            string tableName,
            Func<Task> action,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Centralized, safe SQL Server IDENTITY_INSERT management with validation, provider-awareness,
    /// and strict try/finally cleanup preserving original exceptions.
    /// </summary>
    public class IdentityInsertHelper : IIdentityInsertService
    {
        private static readonly Regex SafeTableRegex = new(
            @"^(?:\[?([a-zA-Z0-9_]+)\]?\.)?\[?([a-zA-Z0-9_]+)\]?$",
            RegexOptions.Compiled);

        private readonly ILogger<IdentityInsertHelper>? _logger;

        public IdentityInsertHelper(ILogger<IdentityInsertHelper>? logger = null)
        {
            _logger = logger;
        }

        public static string SanitizeTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));

            var match = SafeTableRegex.Match(tableName.Trim());
            if (!match.Success)
            {
                throw new ArgumentException(
                    $"Invalid table identifier '{tableName}'. Only alphanumeric characters and underscores are allowed.",
                    nameof(tableName));
            }

            var schema = match.Groups[1].Success ? match.Groups[1].Value : null;
            var table = match.Groups[2].Value;

            return schema != null ? $"[{schema}].[{table}]" : $"[{table}]";
        }

        public async Task ExecuteWithIdentityInsertAsync(
            DbContext db,
            string tableName,
            Func<Task> action,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(action);

            var safeTable = SanitizeTableName(tableName);

            // SQLite and InMemory do not support IDENTITY_INSERT. Run directly without raw SQL.
            if (!db.Database.IsSqlServer())
            {
                await action();
                await db.SaveChangesAsync(ct);
                return;
            }

            var connection = db.Database.GetDbConnection();
            var openedLocally = false;
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await db.Database.OpenConnectionAsync(ct);
                openedLocally = true;
            }

            var identityTurnedOn = false;
            Exception? executionException = null;

            try
            {
                await db.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {safeTable} ON", ct);
                identityTurnedOn = true;

                await action();
                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                executionException = ex;
                throw;
            }
            finally
            {
                if (identityTurnedOn)
                {
                    try
                    {
                        await db.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {safeTable} OFF", ct);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger?.LogError(cleanupEx, "Failed to disable IDENTITY_INSERT on {Table}", safeTable);
                        if (executionException != null)
                        {
                            throw new AggregateException(
                                $"Execution failed and subsequent IDENTITY_INSERT OFF cleanup on {safeTable} also failed.",
                                executionException,
                                cleanupEx);
                        }
                        throw;
                    }
                }

                if (openedLocally && db.Database.CurrentTransaction == null)
                {
                    await db.Database.CloseConnectionAsync();
                }
            }
        }
    }
}
