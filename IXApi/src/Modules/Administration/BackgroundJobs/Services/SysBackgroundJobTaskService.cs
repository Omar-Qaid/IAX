using System.Text.Json;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services;

public sealed class SysBackgroundJobTaskService(IUnitOfWork unitOfWork, ISysBackgroundJobRegistry registry)
    : ISysBackgroundJobTaskService
{
    public async Task<List<SysBackgroundJobTaskDto>?> GetAsync(long jobId, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Context.Set<SysBackgroundJob>()
            .AnyAsync(job => job.RecId == jobId && !job.IsDeleted, cancellationToken)) return null;
        var tasks = await unitOfWork.Context.Set<SysBackgroundJobTask>()
            .Where(task => task.JobId == jobId).OrderBy(task => task.ExecutionOrder)
            .ToListAsync(cancellationToken);
        return tasks.Adapt<List<SysBackgroundJobTaskDto>>();
    }

    public async Task<BatchSetupResult<bool>> SaveAsync(
        long jobId, List<SysBackgroundJobTaskDto> submitted, CancellationToken cancellationToken)
    {
        var job = await unitOfWork.Context.Set<SysBackgroundJob>()
            .FirstOrDefaultAsync(item => item.RecId == jobId && !item.IsDeleted, cancellationToken);
        if (job == null) return new(BatchSetupStatus.NotFound);
        if (job.JobKey != BatchTasksJobHandler.Key || job.IsEnabled || job.MaxRetryCount != 0)
            return new(BatchSetupStatus.Invalid, Error: "Disable the BatchTasks job and set job retries to zero before editing tasks.");
        if (await unitOfWork.Context.Set<SysBackgroundJobExecution>().AnyAsync(execution => execution.JobId == jobId &&
            (execution.Status == SysJobExecutionStatus.Running || execution.Status == SysJobExecutionStatus.Pending),
            cancellationToken))
            return new(BatchSetupStatus.Conflict, Error: "Wait for pending and running executions before editing tasks.");
        if (submitted.Count > 100 || submitted.Select(task => task.ExecutionOrder).Distinct().Count() != submitted.Count ||
            submitted.Where(task => task.RecId > 0).GroupBy(task => task.RecId).Any(group => group.Count() > 1))
            return new(BatchSetupStatus.Invalid, Error: "Use unique task orders and identities, with at most 100 tasks.");

        var set = unitOfWork.Context.Set<SysBackgroundJobTask>();
        var stored = await set.Where(task => task.JobId == jobId).ToListAsync(cancellationToken);
        foreach (var task in submitted)
        {
            if (task.RecId < 0 || task.RecId > 0 && stored.All(item => item.RecId != task.RecId) ||
                task.JobKey == BatchTasksJobHandler.Key || !registry.IsRegistered(task.JobKey))
                return new(BatchSetupStatus.Invalid, Error: "Invalid task identity or handler.");
            if (task.DependsOnTaskId.HasValue && !submitted.Any(item => item.RecId > 0 &&
                item.RecId == task.DependsOnTaskId && item.ExecutionOrder < task.ExecutionOrder &&
                (!task.IsEnabled || item.IsEnabled)))
                return new(BatchSetupStatus.Invalid, Error: "A dependency must be an earlier enabled task in this job.");
            try
            {
                if (!string.IsNullOrWhiteSpace(task.PayloadJson))
                    using (JsonDocument.Parse(task.PayloadJson)) { }
            }
            catch (JsonException)
            {
                return new(BatchSetupStatus.Invalid, Error: "Task parameters must be valid JSON.");
            }
        }

        set.RemoveRange(stored.Where(task => submitted.All(item => item.RecId != task.RecId)));
        foreach (var task in submitted)
        {
            var entity = stored.FirstOrDefault(item => item.RecId == task.RecId) ??
                new SysBackgroundJobTask { JobId = jobId };
            if (entity.RecId == 0) set.Add(entity);
            task.Adapt(entity);
            entity.Name = task.Name.Trim();
        }
        await unitOfWork.Context.SaveChangesAsync(cancellationToken);
        return new(BatchSetupStatus.Ok, true);
    }

    public async Task<List<object>?> HistoryAsync(long jobId, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Context.Set<SysBackgroundJob>()
            .AnyAsync(job => job.RecId == jobId && !job.IsDeleted, cancellationToken)) return null;
        var rows = await unitOfWork.Context.Set<SysBackgroundJobTaskExecution>().AsNoTracking()
            .Where(execution => execution.Execution.JobId == jobId)
            .OrderByDescending(execution => execution.RecId).Take(100)
            .Select(execution => new
            {
                execution.RecId, execution.ExecutionId, execution.TaskId, execution.TaskName,
                execution.Attempt, execution.CorrelationId, execution.Status, execution.StartedAt,
                execution.CompletedAt, execution.Output, execution.ErrorMessage
            }).ToListAsync(cancellationToken);
        return rows.Cast<object>().ToList();
    }
}
