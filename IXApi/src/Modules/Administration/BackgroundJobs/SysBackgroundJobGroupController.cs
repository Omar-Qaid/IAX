using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.Persistence;
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
public sealed class SysBackgroundJobGroupController(
    IAdministrationDataContext db,
    ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet, DomainPermission("System", "BackgroundJobs", "View")]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var company = currentUser.GetDataAreaId();
        var rows = await db.SysBackgroundJobGroups.AsNoTracking()
            .Where(group => group.DataAreaId == company && !group.IsDeleted)
            .OrderBy(group => group.GroupCode)
            .Select(group => ToDto(group)).ToListAsync(ct);
        return Ok(APIResponse<List<SysBackgroundJobGroupDto>>.Ok(rows));
    }

    [HttpPost, DomainPermission("System", "BackgroundJobs", "Create")]
    public async Task<IActionResult> Create(SysBackgroundJobGroupDto dto, CancellationToken ct)
    {
        var company = currentUser.GetDataAreaId();
        var code = dto.GroupCode.Trim();
        if (await db.SysBackgroundJobGroups.AnyAsync(group => group.DataAreaId == company &&
            !group.IsDeleted && group.GroupCode == code, ct))
            return Conflict(APIResponse<SysBackgroundJobGroupDto>.Fail($"Batch group '{code}' already exists."));
        var entity = new SysBackgroundJobGroup
        {
            GroupCode = code, Description = Clean(dto.Description), SchedulingPriority = dto.SchedulingPriority,
            MaxConcurrency = dto.MaxConcurrency, IsActive = dto.IsActive, DataAreaId = company,
            CreatedBy = currentUser.GetCurrentUserId(), CreatedAt = DateTime.UtcNow
        };
        db.SysBackgroundJobGroups.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(APIResponse<SysBackgroundJobGroupDto>.Ok(ToDto(entity), "Batch group created."));
    }

    [HttpPut("{id:long}"), DomainPermission("System", "BackgroundJobs", "Edit")]
    public async Task<IActionResult> Update(long id, SysBackgroundJobGroupDto dto, CancellationToken ct)
    {
        var company = currentUser.GetDataAreaId();
        var entity = await db.SysBackgroundJobGroups.FirstOrDefaultAsync(group => group.RecId == id &&
            group.DataAreaId == company && !group.IsDeleted, ct);
        if (entity == null) return NotFound(APIResponse<SysBackgroundJobGroupDto>.Fail("Batch group not found."));
        var code = dto.GroupCode.Trim();
        if (await db.SysBackgroundJobGroups.AnyAsync(group => group.RecId != id && group.DataAreaId == company &&
            !group.IsDeleted && group.GroupCode == code, ct))
            return Conflict(APIResponse<SysBackgroundJobGroupDto>.Fail($"Batch group '{code}' already exists."));
        entity.GroupCode = code; entity.Description = Clean(dto.Description);
        entity.SchedulingPriority = dto.SchedulingPriority; entity.MaxConcurrency = dto.MaxConcurrency;
        entity.IsActive = dto.IsActive; entity.LastModifiedBy = currentUser.GetCurrentUserId();
        entity.LastModifiedAt = DateTime.UtcNow; entity.RecVersion++;
        await db.SaveChangesAsync(ct);
        return Ok(APIResponse<SysBackgroundJobGroupDto>.Ok(ToDto(entity), "Batch group updated."));
    }

    [HttpDelete("{id:long}"), DomainPermission("System", "BackgroundJobs", "Delete")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var company = currentUser.GetDataAreaId();
        var entity = await db.SysBackgroundJobGroups.FirstOrDefaultAsync(group => group.RecId == id &&
            group.DataAreaId == company && !group.IsDeleted, ct);
        if (entity == null) return NotFound(APIResponse<bool>.Fail("Batch group not found."));
        if (await db.SysBackgroundJobs.AnyAsync(job => !job.IsDeleted && job.BatchGroup == entity.GroupCode, ct))
            return Conflict(APIResponse<bool>.Fail("The batch group is assigned to one or more batch jobs."));
        entity.IsDeleted = true; entity.IsActive = false; entity.LastModifiedAt = DateTime.UtcNow;
        entity.LastModifiedBy = currentUser.GetCurrentUserId();
        await db.SaveChangesAsync(ct);
        return Ok(APIResponse<bool>.Ok(true, "Batch group deleted."));
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static SysBackgroundJobGroupDto ToDto(SysBackgroundJobGroup group) => new()
    {
        RecId = group.RecId, GroupCode = group.GroupCode, Description = group.Description,
        SchedulingPriority = group.SchedulingPriority, MaxConcurrency = group.MaxConcurrency,
        IsActive = group.IsActive, RowVersion = group.RowVersion
    };
}
