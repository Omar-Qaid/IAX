using System.Text.Json;
using IAX.IXApi.Infrastructure.Realtime;
using IAX.IXApi.Modules.Administration.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services
{
    /// <summary>
    /// Execution context handed to a job handler for a single run.
    /// Carries the job identity, the attempt number, the payload, and a scoped
    /// <see cref="IServiceProvider"/> so handlers can resolve any DI service.
    /// </summary>
    public sealed class SysBackgroundJobContext
    {
        public long JobId { get; init; }
        public long TaskId { get; init; }
        public Guid CorrelationId { get; init; } = Guid.NewGuid();
        public long ExecutionId { get; init; }
        public string JobKey { get; init; } = null!;
        public string JobName { get; init; } = null!;
        public string? TenantId { get; init; }
        public int Attempt { get; init; } = 1;

        /// <summary>Raw JSON payload configured on the job (may be null).</summary>
        public string? PayloadJson { get; init; }

        /// <summary>Scoped service provider for resolving dependencies inside the handler.</summary>
        public IServiceProvider Services { get; init; } = null!;

        /// <summary>
        /// Free-form output the handler can set; persisted on the execution record.
        /// </summary>
        public string? Output { get; set; }

        /// <summary>Deserializes the payload JSON into the requested type, or default if absent.</summary>
        public T? GetPayload<T>()
        {
            if (string.IsNullOrWhiteSpace(PayloadJson)) return default;
            return JsonSerializer.Deserialize<T>(PayloadJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        /// <summary>
        /// Publishes a real-time <c>JobProgress</c> update for this execution over SignalR.
        /// Handlers call this to stream progress to clients; it is a no-op if the realtime
        /// service is unavailable, so it never breaks job execution.
        /// </summary>
        public async Task ReportProgressAsync(int percent, string? message = null, CancellationToken ct = default)
        {
            var normalizedPercent = Math.Clamp(percent, 0, 100);
            try
            {
                // Use a separate scope because a batch handler may be using the execution scope's
                // DbContext at the same time. ExecuteUpdate persists progress immediately.
                using var progressScope = Services.CreateScope();
                var db = progressScope.ServiceProvider.GetRequiredService<IAdministrationDataContext>();
                await db.SysBackgroundJobs.IgnoreQueryFilters()
                    .Where(job => job.RecId == JobId && !job.IsDeleted)
                    .ExecuteUpdateAsync(update => update.SetProperty(job => job.Progress, normalizedPercent), ct);

                var realtime = progressScope.ServiceProvider.GetService<ISysRealtimeManager>();
                if (realtime is not null)
                    await realtime.BroadcastAsync(SysRealtimeMessage.JobProgress(new
                    {
                        JobId,
                        ExecutionId,
                        JobName,
                        Percent = normalizedPercent,
                        Message = message,
                    }));
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                // Reporting progress must never fail the batch operation itself.
            }
        }
    }
}
