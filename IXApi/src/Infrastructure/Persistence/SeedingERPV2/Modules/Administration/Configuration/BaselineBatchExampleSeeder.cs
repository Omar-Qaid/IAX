using System.Globalization;
using System.Text;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Administration;

/// <summary>
/// Disabled batch-job examples, each a header with its ordered tasks, from <c>Data/baseline-administration.json</c>.
/// They stay disabled because the enabled jobs already process these queues. A job is written once with its tasks
/// in one save, so a rerun never overwrites an administrator's edit or restores a deleted example.
/// </summary>
public sealed class BaselineBatchExampleSeeder : IBaselineSeeder
{
    public const string SeederId = "Administration.BatchExamples";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineSeedingRegistration.Settings];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        foreach (var example in BaselineManifest.Read<Catalog>("baseline-administration.json").BatchExamples)
        {
            if (await c.Db.Set<SysBackgroundJob>().IgnoreQueryFilters().AnyAsync(x => x.Caption == example.Name && x.DataAreaId == c.Company, ct))
                continue;
            var job = new SysBackgroundJob
            {
                Caption = example.Name, Description = example.Description, JobKey = BatchTasksJobHandler.Key, DataAreaId = c.Company,
                ScheduleType = SysJobScheduleType.Recurring, Status = SysJobStatus.Active, IsEnabled = false, PreventOverlap = true,
                RecurrenceData = Encoding.UTF8.GetBytes(example.IntervalSeconds.ToString(CultureInfo.InvariantCulture)),
                MaxRetryCount = 0, TimeoutSeconds = 900, StartDateTime = null, PayloadJson = "{}"
            };
            c.Db.Set<SysBackgroundJob>().Add(job);
            for (var index = 0; index < example.Tasks.Length; index++)
                c.Db.Set<SysBackgroundJobTask>().Add(new SysBackgroundJobTask
                {
                    Job = job, Name = example.Tasks[index].Name, JobKey = example.Tasks[index].ServiceKey, ExecutionOrder = index + 1,
                    IsEnabled = true, PayloadJson = "{}", MaxRetryCount = 0, RetryDelaySeconds = 60
                });
            await c.Db.SaveChangesAsync(ct);
        }
    }

    private sealed record Catalog(BatchExample[] BatchExamples);
    private sealed record BatchExample(string Name, int IntervalSeconds, string Description, BatchTask[] Tasks);
    private sealed record BatchTask(string Name, string ServiceKey);
}
