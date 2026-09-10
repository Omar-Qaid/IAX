using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Workflow.Persistence;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Modules.Workflow.Scheduling;

public sealed class WFProcessScheduledJobHandler : ISysBackgroundJobHandler
{
    public string JobKey => "WFProcessScheduled";

    public async Task ExecuteAsync(SysBackgroundJobContext context, CancellationToken ct)
    {
        // Every retry gets fresh tracking, identity and transaction state.
        using var strategyScope = context.Services.CreateScope();
        var strategyDb = strategyScope.ServiceProvider.GetRequiredService<IWorkflowDataContext>();
        await strategyDb.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            using var scope = context.Services.CreateScope();
            var services = scope.ServiceProvider;
            var db = services.GetRequiredService<IWorkflowDataContext>();
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            // Hold the schedule row until both request and occurrence advancement commit.
            // Competing executions wait, then see the advanced NextRunAt and do no work.
            var row = await db.Set<WFProcessScheduled>().FromSqlInterpolated(
                $"SELECT * FROM [WFProcessScheduled] WITH (UPDLOCK, ROWLOCK) WHERE [BackgroundJobId] = {context.JobId}")
                .SingleOrDefaultAsync(ct);
            var now = DateTime.UtcNow;
            if (row is null || !row.Enabled || row.NextRunAt is null || row.NextRunAt > now)
            {
                context.Output = "No workflow occurrence is due.";
                await transaction.CommitAsync(ct);
                return;
            }
            await ProcessScheduleAccount.ValidateAsync(services.GetRequiredService<UserManager<AspNetUser>>(),
                services.GetRequiredService<IAppPermissionService>(), row.ExecutionUserId, row.DataAreaId, ct);
            services.GetRequiredService<BackgroundExecutionIdentity>()
                .Initialize(row.ExecutionUserId, row.DataAreaId, row.OwnerAccountId);
            var configuration = WFProcessScheduledController.Read(row);
            var submission = await ProcessScheduleSource.ResolveAsync(db, row.ProcessId, row.DataAreaId, configuration, ct);
            var result = await services.GetRequiredService<IWfRequestService>().SubmitDynamicAsync(submission, ct);
            row.LastRequestId = result.RequestId;
            row.LastRunAt = now;
            row.NextRunAt = WorkflowRecurrence.NextUtc(configuration.StartsAt, configuration.TimeZone, configuration.Frequency, DateTime.UtcNow);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            context.Output = $"Created workflow request {result.RequestId}.";
        });
    }
}
