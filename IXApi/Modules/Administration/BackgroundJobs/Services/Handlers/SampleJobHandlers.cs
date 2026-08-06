using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers
{
    /// <summary>
    /// Sample handler: nightly cleanup of expired notifications.
    /// Demonstrates resolving a scoped DbContext from the execution context.
    /// Bind a job to this with JobKey = "CleanupExpiredNotifications".
    /// </summary>
    public class CleanupExpiredNotificationsJobHandler : ISysBackgroundJobHandler
    {
        public string JobKey => "CleanupExpiredNotifications";

        public async Task ExecuteAsync(SysBackgroundJobContext context, CancellationToken cancellationToken)
        {
            var db = context.Services.GetRequiredService<ApplicationDbContext>();
            var now = DateTime.UtcNow;

            var expired = await db.Set<SysNotification>()
                .Where(n => n.ExpiryDate != null && n.ExpiryDate <= now
                         && !n.IsDeleted && n.Status != SysNotificationStatus.Expired)
                .Take(500)
                .ToListAsync(cancellationToken);

            foreach (var n in expired)
                n.Status = SysNotificationStatus.Expired;

            if (expired.Count > 0)
                await db.SaveChangesAsync(cancellationToken);

            context.Output = $"Expired {expired.Count} notification(s).";
        }
    }

    /// <summary>
    /// Sample handler accepting a typed JSON payload.
    /// Bind a job to this with JobKey = "SampleEcho" and a PayloadJson like
    /// {"Label":"hello","Iterations":3}.
    /// </summary>
    public class SampleEchoJobHandler : ISysBackgroundJobHandler
    {
        private readonly ILogger<SampleEchoJobHandler> _logger;

        public SampleEchoJobHandler(ILogger<SampleEchoJobHandler> logger) => _logger = logger;

        public string JobKey => "SampleEcho";

        public sealed class Payload
        {
            public string Label { get; set; } = "tick";
            public int Iterations { get; set; } = 1;
        }

        public async Task ExecuteAsync(SysBackgroundJobContext context, CancellationToken cancellationToken)
        {
            var payload = context.GetPayload<Payload>() ?? new Payload();
            for (var i = 0; i < Math.Max(1, payload.Iterations); i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _logger.LogInformation("[SampleEcho] {Label} ({Attempt}) iteration {I}", payload.Label, context.Attempt, i + 1);
                await Task.Delay(200, cancellationToken);
            }
            context.Output = $"Echoed '{payload.Label}' x{payload.Iterations}.";
        }
    }
}

