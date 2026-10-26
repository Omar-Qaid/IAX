using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Core
{
    /// <summary>
    /// Distributed lock provider using SQL Server's sp_getapplock for cross-process concurrency safety,
    /// with an in-memory SemaphoreSlim fallback when running against SQLite or InMemory providers.
    /// </summary>
    public class SqlServerSeederLock : ISeederLock
    {
        private const string LockResourceName = "IXApi_DatabaseSeeder";
        private static readonly SemaphoreSlim ProcessLock = new(1, 1);

        private readonly ApplicationDbContext _db;
        private readonly ILogger<SqlServerSeederLock>? _logger;

        public SqlServerSeederLock(ApplicationDbContext db, ILogger<SqlServerSeederLock>? logger = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<IAsyncDisposable> AcquireLockAsync(int timeoutSeconds, CancellationToken cancellationToken)
        {
            var timeoutMs = Math.Max(1, timeoutSeconds) * 1000;

            // 1. In-process serialization first
            var acquiredProcessLock = await ProcessLock.WaitAsync(timeoutMs, cancellationToken);
            if (!acquiredProcessLock)
            {
                throw new TimeoutException($"Failed to acquire in-process startup lock within {timeoutSeconds} seconds.");
            }

            // 2. If not SQL Server (SQLite, InMemory in unit tests), in-process lock is sufficient
            if (!_db.Database.IsSqlServer())
            {
                return new LockReleaser(() =>
                {
                    ProcessLock.Release();
                    return ValueTask.CompletedTask;
                });
            }

            // 3. SQL Server distributed application lock (Session-scoped)
            DbConnection connection = _db.Database.GetDbConnection();
            bool openedLocally = false;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync(cancellationToken);
                    openedLocally = true;
                }

                await using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    DECLARE @result INT;
                    EXEC @result = sp_getapplock
                        @Resource = @res,
                        @LockMode = 'Exclusive',
                        @LockOwner = 'Session',
                        @LockTimeout = @timeout;
                    SELECT @result;";

                var pRes = cmd.CreateParameter();
                pRes.ParameterName = "@res";
                pRes.Value = LockResourceName;
                cmd.Parameters.Add(pRes);

                var pTimeout = cmd.CreateParameter();
                pTimeout.ParameterName = "@timeout";
                pTimeout.Value = timeoutMs;
                cmd.Parameters.Add(pTimeout);

                var resultObj = await cmd.ExecuteScalarAsync(cancellationToken);
                var resultCode = Convert.ToInt32(resultObj);

                // sp_getapplock return values:
                // >= 0: The lock was successfully granted (0 = synchronous, 1 = after waiting)
                // -1: The lock request timed out.
                // -2: The lock request was canceled.
                // -3: The lock request was chosen as a deadlock victim.
                // -999: Parameter validation or other error.
                if (resultCode < 0)
                {
                    throw new InvalidOperationException(
                        $"Failed to acquire distributed SQL Server application lock '{LockResourceName}'. Result code: {resultCode}.");
                }

                _logger?.LogInformation("Successfully acquired distributed startup lock '{LockResource}'.", LockResourceName);

                return new LockReleaser(async () =>
                {
                    try
                    {
                        if (connection.State == ConnectionState.Open)
                        {
                            await using var relCmd = connection.CreateCommand();
                            relCmd.CommandText = "EXEC sp_releaseapplock @Resource = @res, @LockOwner = 'Session';";
                            var p = relCmd.CreateParameter();
                            p.ParameterName = "@res";
                            p.Value = LockResourceName;
                            relCmd.Parameters.Add(p);
                            await relCmd.ExecuteNonQueryAsync(CancellationToken.None);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Exception releasing sp_releaseapplock for {Resource}", LockResourceName);
                    }
                    finally
                    {
                        if (openedLocally && connection.State == ConnectionState.Open)
                        {
                            await connection.CloseAsync();
                        }
                        ProcessLock.Release();
                    }
                });
            }
            catch
            {
                if (openedLocally && connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
                ProcessLock.Release();
                throw;
            }
        }

        private sealed class LockReleaser : IAsyncDisposable
        {
            private readonly Func<ValueTask> _releaseAction;
            private int _disposed;

            public LockReleaser(Func<ValueTask> releaseAction)
            {
                _releaseAction = releaseAction;
            }

            public async ValueTask DisposeAsync()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 0)
                {
                    await _releaseAction();
                }
            }
        }
    }
}
