using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;
using IAX.IXApi.Modules.Administration.Persistence;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs;

public sealed class SysBackgroundJobTaskDto
{
    public long RecId { get; set; }
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(200), System.Text.Json.Serialization.JsonPropertyName("serviceKey")]
    public string JobKey { get; set; } = string.Empty;
    public string? PayloadJson { get; set; }
    [Range(1, int.MaxValue)] public int ExecutionOrder { get; set; } = 1;
    public long? DependsOnTaskId { get; set; }
    public bool IsEnabled { get; set; } = true;
    [Range(0, 10)] public int MaxRetryCount { get; set; }
    [Range(1, 3600)] public int RetryDelaySeconds { get; set; } = 60;
}

[ApiController, Authorize]
[Route("api/v1/SysBackgroundJob/{jobId:long}/tasks")]
public sealed class SysBackgroundJobTaskController(IAdministrationDataContext db, ISysBackgroundJobRegistry registry) : ControllerBase
{
    [HttpGet, DomainPermission("System", "BackgroundJobs", "View")]
    public async Task<IActionResult> Get(long jobId, CancellationToken ct)
    {
        if (!await db.SysBackgroundJobs.AnyAsync(j => j.RecId == jobId && !j.IsDeleted, ct)) return NotFound();
        var tasks = await db.SysBackgroundJobTasks.Where(t => t.JobId == jobId)
            .OrderBy(t => t.ExecutionOrder).Select(t => new SysBackgroundJobTaskDto
            {
                RecId = t.RecId, Name = t.Name, JobKey = t.JobKey, PayloadJson = t.PayloadJson,
                ExecutionOrder = t.ExecutionOrder, DependsOnTaskId = t.DependsOnTaskId, IsEnabled = t.IsEnabled,
                MaxRetryCount = t.MaxRetryCount, RetryDelaySeconds = t.RetryDelaySeconds
            }).ToListAsync(ct);
        return Ok(APIResponse<List<SysBackgroundJobTaskDto>>.Ok(tasks));
    }

    [HttpPut, DomainPermission("System", "BackgroundJobs", "Edit")]
    public async Task<IActionResult> Save(long jobId, List<SysBackgroundJobTaskDto> submitted, CancellationToken ct)
    {
        var job = await db.SysBackgroundJobs.FirstOrDefaultAsync(j => j.RecId == jobId && !j.IsDeleted, ct);
        if (job == null) return NotFound();
        if (job.JobKey != BatchTasksJobHandler.Key || job.IsEnabled || job.MaxRetryCount != 0)
            return BadRequest(APIResponse<bool>.Fail("Disable the BatchTasks job and set job retries to zero before editing tasks."));
        if (await db.SysBackgroundJobExecutions.AnyAsync(e => e.JobId == jobId &&
            (e.Status == SysJobExecutionStatus.Running || e.Status == SysJobExecutionStatus.Pending), ct))
            return Conflict(APIResponse<bool>.Fail("Wait for pending and running executions before editing tasks."));
        if (submitted.Count > 100 || submitted.Select(t => t.ExecutionOrder).Distinct().Count() != submitted.Count ||
            submitted.Where(t => t.RecId > 0).GroupBy(t => t.RecId).Any(g => g.Count() > 1))
            return BadRequest(APIResponse<bool>.Fail("Use unique task orders and identities, with at most 100 tasks."));
        var stored = await db.SysBackgroundJobTasks.Where(t => t.JobId == jobId).ToListAsync(ct);
        foreach (var task in submitted)
        {
            if (task.RecId < 0 || task.RecId > 0 && stored.All(t => t.RecId != task.RecId) ||
                task.JobKey == BatchTasksJobHandler.Key || !registry.IsRegistered(task.JobKey))
                return BadRequest(APIResponse<bool>.Fail("Invalid task identity or handler."));
            if (task.DependsOnTaskId.HasValue && !submitted.Any(t => t.RecId > 0 && t.RecId == task.DependsOnTaskId &&
                t.ExecutionOrder < task.ExecutionOrder && (!task.IsEnabled || t.IsEnabled)))
                return BadRequest(APIResponse<bool>.Fail("A dependency must be an earlier enabled task in this job."));
            try { if (!string.IsNullOrWhiteSpace(task.PayloadJson)) { using var json = JsonDocument.Parse(task.PayloadJson); } }
            catch (JsonException) { return BadRequest(APIResponse<bool>.Fail("Task parameters must be valid JSON.")); }
        }
        db.SysBackgroundJobTasks.RemoveRange(stored.Where(t => submitted.All(s => s.RecId != t.RecId)));
        foreach (var task in submitted)
        {
            var entity = stored.FirstOrDefault(t => t.RecId == task.RecId) ?? new SysBackgroundJobTask { JobId = jobId };
            if (entity.RecId == 0) db.SysBackgroundJobTasks.Add(entity);
            entity.Name = task.Name.Trim(); entity.JobKey = task.JobKey;
            entity.PayloadJson = task.PayloadJson; entity.ExecutionOrder = task.ExecutionOrder;
            entity.DependsOnTaskId = task.DependsOnTaskId; entity.IsEnabled = task.IsEnabled;
            entity.MaxRetryCount = task.MaxRetryCount; entity.RetryDelaySeconds = task.RetryDelaySeconds;
        }
        await db.SaveChangesAsync(ct);
        return await Get(jobId, ct);
    }

    [HttpGet("history"), DomainPermission("System", "BackgroundJobs", "View")]
    public async Task<IActionResult> History(long jobId, CancellationToken ct)
    {
        if (!await db.SysBackgroundJobs.AnyAsync(j => j.RecId == jobId && !j.IsDeleted, ct)) return NotFound();
        var rows = await db.SysBackgroundJobTaskExecutions.AsNoTracking()
            .Where(e => e.Execution.JobId == jobId).OrderByDescending(e => e.RecId).Take(100)
            .Select(e => new { e.RecId, e.ExecutionId, e.TaskId, e.TaskName, e.Attempt, e.CorrelationId, e.Status, e.StartedAt, e.CompletedAt, e.Output, e.ErrorMessage }).ToListAsync(ct);
        return Ok(APIResponse<object>.Ok(rows));
    }
}
