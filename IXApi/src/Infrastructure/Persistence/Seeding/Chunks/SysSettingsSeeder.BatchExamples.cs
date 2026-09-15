using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

public partial class SettingsSeeder
{
    // Examples remain disabled: the enabled production sweeps already process these queues.
    // Rerunning the seeder must not overwrite an administrator's edits or restore deleted tasks.
    private static async Task SeedBatchExamplesAsync(ApplicationDbContext db, CancellationToken ct)
    {
        await SeedBatchExampleAsync(db, "Example - Process request submission", 900,
            "Disabled scaffold. Implement WorkflowRequestSubmissionBatchService.ProcessAsync and configure task parameters before enabling.",
            [("Submit process requests", IAX.IXApi.Modules.Workflow.Jobs.WorkflowRequestSubmissionBatchService.ServiceKey)], ct);
        await SeedBatchExampleAsync(db, "Example - Activity notification delivery", 60,
            "Disabled example. Uses activity-configured queued channels. Do not enable alongside the existing delivery jobs.",
            [("System notifications", "WorkflowNotification"), ("Email delivery", "EmailDelivery"),
             ("SMS delivery", "SmsDelivery")], ct);
        await SeedBatchExampleAsync(db, "Example - Activity auto-pass", 900,
            "Disabled example. Checks AutoPassing and AutoPassingHrs on assignments; the interval is the sweep frequency, not the activity deadline. Do not enable alongside WfActivityAutoPass.",
            [("Process due activity auto-pass", "WfActivityAutoPass")], ct);

        await SeedBatchExampleAsync(db, "Example - Push notification delivery", 60,
            "Disabled example. Delivers queued push notifications. Configure the transport before enabling; do not duplicate PushNotification.",
            [("Deliver push notifications", "PushNotification")], ct);
        await SeedBatchExampleAsync(db, "Example - Other notification delivery", 60,
            "Disabled example. Processes other supported queued notification channels; do not duplicate NotificationDelivery.",
            [("Deliver other notifications", "NotificationDelivery")], ct);
        await SeedBatchExampleAsync(db, "Example - Workflow reminders", 300,
            "Disabled example. Processes due queued workflow reminders; do not duplicate WorkflowReminder.",
            [("Process workflow reminders", "WorkflowReminder")], ct);
        await SeedBatchExampleAsync(db, "Example - Workflow escalations", 300,
            "Disabled example. Processes queued escalation notifications; do not duplicate WorkflowEscalation.",
            [("Process workflow escalations", "WorkflowEscalation")], ct);
        await SeedBatchExampleAsync(db, "Example - Expired notification cleanup", 86400,
            "Disabled example. Marks expired notifications without deleting them; do not duplicate WorkflowCleanup.",
            [("Clean expired notifications", "WorkflowCleanup")], ct);
    }

    private static async Task SeedBatchExampleAsync(ApplicationDbContext db, string name, int intervalSeconds,
        string description, (string Name, string ServiceKey)[] tasks, CancellationToken ct)
    {
        var company = IAX.IXApi.Shared.Application.Identity.CompanyContextDefaults.DataAreaId;
        if (await db.Set<SysBackgroundJob>().IgnoreQueryFilters()
            .AnyAsync(job => job.Caption == name && job.DataAreaId == company, ct)) return;

        var job = new SysBackgroundJob
        {
            Caption = name, Description = description, JobKey = BatchTasksJobHandler.Key,
            DataAreaId = company, ScheduleType = SysJobScheduleType.Recurring,
            RecurrenceData = System.Text.Encoding.UTF8.GetBytes(intervalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)), Status = SysJobStatus.Active,
            IsEnabled = false, PreventOverlap = true, MaxRetryCount = 0,
            TimeoutSeconds = 900, StartDateTime = null, PayloadJson = "{}"
        };
        db.Set<SysBackgroundJob>().Add(job);
        for (var index = 0; index < tasks.Length; index++)
        {
            db.Set<SysBackgroundJobTask>().Add(new SysBackgroundJobTask
            {
                Job = job, Name = tasks[index].Name, JobKey = tasks[index].ServiceKey,
                ExecutionOrder = index + 1, IsEnabled = true, PayloadJson = "{}",
                MaxRetryCount = 0, RetryDelaySeconds = 60
            });
        }
        // Save the header and task graph together so a failed seed cannot leave a partial example.
        await db.SaveChangesAsync(ct);
    }
}
