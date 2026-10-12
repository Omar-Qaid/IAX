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
public sealed class SysBackgroundJobTaskController(ISysBackgroundJobTaskService tasks) : ControllerBase
{
    [HttpGet, DomainPermission("System", "BackgroundJobs", "View")]
    public async Task<IActionResult> Get(long jobId, CancellationToken ct)
    {
        var result = await tasks.GetAsync(jobId, ct);
        return result == null ? NotFound() : Ok(APIResponse<List<SysBackgroundJobTaskDto>>.Ok(result));
    }

    [HttpPut, DomainPermission("System", "BackgroundJobs", "Edit")]
    public async Task<IActionResult> Save(long jobId, List<SysBackgroundJobTaskDto> submitted, CancellationToken ct)
    {
        var result = await tasks.SaveAsync(jobId, submitted, ct);
        return result.Status switch
        {
            BatchSetupStatus.NotFound => NotFound(),
            BatchSetupStatus.Invalid => BadRequest(APIResponse<bool>.Fail(result.Error!)),
            BatchSetupStatus.Conflict => Conflict(APIResponse<bool>.Fail(result.Error!)),
            _ => await Get(jobId, ct)
        };
    }

    [HttpGet("history"), DomainPermission("System", "BackgroundJobs", "View")]
    public async Task<IActionResult> History(long jobId, CancellationToken ct)
    {
        var result = await tasks.HistoryAsync(jobId, ct);
        return result == null ? NotFound() : Ok(APIResponse<object>.Ok(result));
    }
}
