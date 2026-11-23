using System.Globalization;
using System.Text;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Administration;

/// <summary>
/// The recurring jobs the application needs running: the queued delivery workers and the workflow
/// sweeps. The job caption carries a unique index with no company in it, so a job belongs to the
/// installation and is written once; a rerun never re-enables a job an administrator stopped and never
/// restores one they deleted. Disabled demonstration jobs are not seeded here.
/// </summary>
public sealed class SysBackgroundJobSeeder : IDataSeeder
{
    public string Id => "ERPV2.SysBackgroundJobSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.SysSettingsSeeder"];

    /// <summary>Caption, job key, recurrence in seconds, retry count and description.</summary>
    private static readonly (string JobKey, int IntervalSeconds, int MaxRetryCount, int TimeoutSeconds, string Description)[] Jobs =
    [
        ("EmailDelivery", 60, 0, 900, "Processes the persisted email delivery queue."),
        ("SmsDelivery", 60, 0, 900, "Processes the persisted SMS delivery queue."),
        ("PushNotification", 60, 0, 900, "Processes the persisted push notification queue."),
        ("NotificationDelivery", 60, 0, 900, "Processes the remaining persisted notification delivery queues."),
        ("WorkflowNotification", 60, 0, 900, "Delivers the notifications a workflow activity queued."),
        ("WorkflowReminder", 300, 0, 900, "Processes due workflow reminders."),
        ("WorkflowEscalation", 300, 0, 900, "Processes due workflow escalations."),
        ("WorkflowCleanup", 86400, 0, 900, "Marks expired notifications without deleting them."),
        ("WfActivityAutoPass", 900, 1, 300, "Auto-finishes workflow assignments whose AutoPassingHrs window has elapsed.")
    ];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var existing = await c.Db.Set<SysBackgroundJob>().IgnoreQueryFilters()
            .Select(x => x.JobKey).ToListAsync(ct);
        var keys = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var job in Jobs)
        {
            if (keys.Contains(job.JobKey)) continue;
            c.Db.Set<SysBackgroundJob>().Add(new SysBackgroundJob
            {
                Caption = job.JobKey,
                JobKey = job.JobKey,
                Description = job.Description,
                ScheduleType = SysJobScheduleType.Recurring,
                RecurrenceData = Encoding.UTF8.GetBytes(job.IntervalSeconds.ToString(CultureInfo.InvariantCulture)),
                Status = SysJobStatus.Active,
                IsEnabled = true,
                PreventOverlap = true,
                MaxRetryCount = job.MaxRetryCount,
                RetryDelaySeconds = 60,
                TimeoutSeconds = job.TimeoutSeconds,
                StartDateTime = DateTime.UtcNow,
                DataAreaId = c.Company,
                CreatedBy = c.Actor,
                LastModifiedBy = c.Actor,
                OwnerAccountId = c.Actor
            });
        }
        await c.Db.SaveChangesAsync(ct);
    }
}
