using IAX.IXApi.Modules.Administration.Persistence;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;

/// <summary>Runs the configured task sequence through the existing registered handlers.</summary>
public sealed class BatchTasksJobHandler : ISysBackgroundJobHandler
{
    public const string Key = "BatchTasks";
    public string JobKey => Key;
    public bool SupportsWholeJobRetry => false;

    public async Task ExecuteAsync(SysBackgroundJobContext context, CancellationToken cancellationToken)
    {
        var db = context.Services.GetRequiredService<IAdministrationDataContext>();
        var registry = context.Services.GetRequiredService<ISysBackgroundJobRegistry>();
        var tasks = await db.SysBackgroundJobTasks.AsNoTracking()
            .Where(t => t.JobId == context.JobId && t.IsEnabled)
            .OrderBy(t => t.ExecutionOrder).ThenBy(t => t.RecId).ToListAsync(cancellationToken);
        if (tasks.Count == 0) throw new InvalidOperationException("The batch job has no enabled tasks.");
        var completed = new HashSet<long>();
        foreach (var task in tasks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (task.DependsOnTaskId.HasValue && !completed.Contains(task.DependsOnTaskId.Value))
                throw new InvalidOperationException($"Task {task.Name} depends on an unavailable or later task.");
            if (task.JobKey == Key) throw new InvalidOperationException("Nested batch task runners are not supported.");
            for (var attempt = 1; attempt <= task.MaxRetryCount + 1; attempt++)
            {
                using var attemptScope = context.Services.CreateScope();
                var handler = registry.Resolve(task.JobKey, attemptScope.ServiceProvider)
                    ?? throw new InvalidOperationException($"Unknown task handler: {task.JobKey}.");
                var history = new SysBackgroundJobTaskExecution
                {
                    ExecutionId = context.ExecutionId, TaskId = task.RecId, TaskName = task.Name, Attempt = attempt,
                    StartedAt = DateTime.UtcNow, Status = SysJobExecutionStatus.Running
                };
                db.SysBackgroundJobTaskExecutions.Add(history);
                await db.SaveChangesAsync(cancellationToken);
                var taskContext = new SysBackgroundJobContext
                {
                    JobId = context.JobId, TaskId = task.RecId, CorrelationId = history.CorrelationId,
                    ExecutionId = context.ExecutionId, JobKey = task.JobKey,
                    JobName = task.Name, TenantId = context.TenantId, Attempt = attempt,
                    PayloadJson = task.PayloadJson, Services = attemptScope.ServiceProvider
                };
                try
                {
                    await handler.ExecuteAsync(taskContext, cancellationToken);
                    history.Status = SysJobExecutionStatus.Completed;
                    history.Output = taskContext.Output;
                    completed.Add(task.RecId);
                }
                catch (Exception ex)
                {
                    history.Status = ex is OperationCanceledException ? SysJobExecutionStatus.Cancelled : SysJobExecutionStatus.Failed;
                    history.ErrorMessage = ex.Message;
                    if (ex is OperationCanceledException || attempt > task.MaxRetryCount) throw;
                }
                finally
                {
                    history.CompletedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync(CancellationToken.None);
                }
                if (history.Status == SysJobExecutionStatus.Completed) break;
                await Task.Delay(TimeSpan.FromSeconds(Math.Clamp(task.RetryDelaySeconds, 1, 3600)), cancellationToken);
            }
        }
        context.Output = $"Completed {completed.Count} batch tasks.";
    }
}
