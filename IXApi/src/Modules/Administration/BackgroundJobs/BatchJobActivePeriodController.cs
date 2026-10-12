using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs;

public sealed class BatchJobActivePeriodDto
{
    public long RecId { get; set; }
    [Required, MaxLength(10)] public string Id { get; set; } = string.Empty;
    [MaxLength(150)] public string? Name { get; set; }
    [Range(0, 86399)] public int FromTimeUtc { get; set; }
    [Range(0, 86399)] public int ToTimeUtc { get; set; }
    [Range(0, 86399)] public int FromTimeLocal { get; set; }
    [Range(0, 86399)] public int ToTimeLocal { get; set; }
    public int TimeZoneFollowed { get; set; }
    public bool IsActive { get; set; } = true;
}

[ApiController, Authorize, Route("api/v1/[controller]")]
public sealed class BatchJobActivePeriodController(IBatchJobActivePeriodService periods) : ControllerBase
{
    [HttpGet, DomainPermission("System", "BackgroundJobs", "View")]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(APIResponse<List<BatchJobActivePeriodDto>>.Ok(await periods.GetAsync(ct)));

    [HttpPost, DomainPermission("System", "BackgroundJobs", "Create")]
    public async Task<IActionResult> Create(BatchJobActivePeriodDto dto, CancellationToken ct)
    {
        var result = await periods.CreateAsync(dto, ct);
        return result.Status == BatchSetupStatus.Conflict
            ? Conflict(APIResponse<BatchJobActivePeriodDto>.Fail(result.Error!))
            : Ok(APIResponse<BatchJobActivePeriodDto>.Ok(result.Value!, "Active period created."));
    }

    [HttpPut("{recId:long}"), DomainPermission("System", "BackgroundJobs", "Edit")]
    public async Task<IActionResult> Update(long recId, BatchJobActivePeriodDto dto, CancellationToken ct)
    {
        var result = await periods.UpdateAsync(recId, dto, ct);
        return result.Status switch
        {
            BatchSetupStatus.NotFound => NotFound(APIResponse<BatchJobActivePeriodDto>.Fail(result.Error!)),
            BatchSetupStatus.Conflict => Conflict(APIResponse<BatchJobActivePeriodDto>.Fail(result.Error!)),
            _ => Ok(APIResponse<BatchJobActivePeriodDto>.Ok(result.Value!, "Active period updated."))
        };
    }

    [HttpDelete("{recId:long}"), DomainPermission("System", "BackgroundJobs", "Delete")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var result = await periods.DeleteAsync(recId, ct);
        return result.Status switch
        {
            BatchSetupStatus.NotFound => NotFound(APIResponse<bool>.Fail(result.Error!)),
            BatchSetupStatus.Conflict => Conflict(APIResponse<bool>.Fail(result.Error!)),
            _ => Ok(APIResponse<bool>.Ok(true, "Active period deleted."))
        };
    }
}
