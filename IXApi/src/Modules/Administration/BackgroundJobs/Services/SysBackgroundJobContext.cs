using System.Text.Json;
using IAX.IXApi.Infrastructure.Realtime;
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
            var realtime = Services.GetService<ISysRealtimeManager>();
            if (realtime is null) return;

            await realtime.BroadcastAsync(SysRealtimeMessage.JobProgress(new
            {
                JobId,
                ExecutionId,
                JobName,
                Percent = Math.Clamp(percent, 0, 100),
                Message = message,
            }));
        }
    }
}
