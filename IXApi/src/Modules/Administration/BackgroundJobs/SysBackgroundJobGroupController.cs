using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs;

public sealed class SysBackgroundJobGroupDto
{
    public long RecId { get; set; }
    [Required, MaxLength(10)] public string GroupCode { get; set; } = string.Empty;
    [MaxLength(60)] public string? Description { get; set; }
    [Range(0, 2)] public int SchedulingPriority { get; set; } = 1;
    [Range(1, 100)] public int MaxConcurrency { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public byte[]? RowVersion { get; set; }
}

[ApiController, Authorize]
[Route("api/v1/[controller]")]
public sealed class SysBackgroundJobGroupController(ISysBackgroundJobGroupService groups) : ControllerBase
{
    [HttpGet, DomainPermission("System", "BackgroundJobs", "View")]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(APIResponse<List<SysBackgroundJobGroupDto>>.Ok(await groups.GetAsync(ct)));

    [HttpPost, DomainPermission("System", "BackgroundJobs", "Create")]
    public async Task<IActionResult> Create(SysBackgroundJobGroupDto dto, CancellationToken ct)
    {
        var result = await groups.CreateAsync(dto, ct);
        return result.Status == BatchSetupStatus.Conflict
            ? Conflict(APIResponse<SysBackgroundJobGroupDto>.Fail(result.Error!))
            : Ok(APIResponse<SysBackgroundJobGroupDto>.Ok(result.Value!, "Batch group created."));
    }

    [HttpPut("{id:long}"), DomainPermission("System", "BackgroundJobs", "Edit")]
    public async Task<IActionResult> Update(long id, SysBackgroundJobGroupDto dto, CancellationToken ct)
    {
        var result = await groups.UpdateAsync(id, dto, ct);
        return result.Status switch
        {
            BatchSetupStatus.NotFound => NotFound(APIResponse<SysBackgroundJobGroupDto>.Fail(result.Error!)),
            BatchSetupStatus.Conflict => Conflict(APIResponse<SysBackgroundJobGroupDto>.Fail(result.Error!)),
            _ => Ok(APIResponse<SysBackgroundJobGroupDto>.Ok(result.Value!, "Batch group updated."))
        };
    }

    [HttpDelete("{id:long}"), DomainPermission("System", "BackgroundJobs", "Delete")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await groups.DeleteAsync(id, ct);
        return result.Status switch
        {
            BatchSetupStatus.NotFound => NotFound(APIResponse<bool>.Fail(result.Error!)),
            BatchSetupStatus.Conflict => Conflict(APIResponse<bool>.Fail(result.Error!)),
            _ => Ok(APIResponse<bool>.Ok(true, "Batch group deleted."))
        };
    }
}
