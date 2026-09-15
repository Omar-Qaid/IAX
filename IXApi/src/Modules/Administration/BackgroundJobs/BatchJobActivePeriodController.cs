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
public sealed class BatchJobActivePeriodController(IAdministrationDataContext db, ICurrentUserService user) : ControllerBase
{
    [HttpGet, DomainPermission("System", "BackgroundJobs", "View")]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var company = user.GetDataAreaId();
        var entities = await db.BatchJobActivePeriods.AsNoTracking().Where(x => x.DataAreaId == company && !x.IsDeleted)
            .OrderBy(x => x.Code).ToListAsync(ct);
        return Ok(APIResponse<List<BatchJobActivePeriodDto>>.Ok(entities.Select(Map).ToList()));
    }

    [HttpPost, DomainPermission("System", "BackgroundJobs", "Create")]
    public async Task<IActionResult> Create(BatchJobActivePeriodDto dto, CancellationToken ct)
    {
        var company = user.GetDataAreaId(); var id = dto.Id.Trim();
        if (await db.BatchJobActivePeriods.AnyAsync(x => x.DataAreaId == company && !x.IsDeleted && x.Code == id, ct))
            return Conflict(APIResponse<BatchJobActivePeriodDto>.Fail($"Active period '{id}' already exists."));
        var entity = new SysBackgroundJobActivePeriod { Code = id, Name = Clean(dto.Name), FromTimeUtc = dto.FromTimeUtc,
            ToTimeUtc = dto.ToTimeUtc, FromTimeLocal = dto.FromTimeLocal, ToTimeLocal = dto.ToTimeLocal,
            TimeZoneFollowed = dto.TimeZoneFollowed, IsActive = dto.IsActive, DataAreaId = company,
            CreatedBy = user.GetCurrentUserId(), CreatedAt = DateTime.UtcNow };
        db.BatchJobActivePeriods.Add(entity); await db.SaveChangesAsync(ct);
        return Ok(APIResponse<BatchJobActivePeriodDto>.Ok(Map(entity), "Active period created."));
    }

    [HttpPut("{recId:long}"), DomainPermission("System", "BackgroundJobs", "Edit")]
    public async Task<IActionResult> Update(long recId, BatchJobActivePeriodDto dto, CancellationToken ct)
    {
        var company = user.GetDataAreaId();
        var entity = await db.BatchJobActivePeriods.FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == company && !x.IsDeleted, ct);
        if (entity == null) return NotFound(APIResponse<BatchJobActivePeriodDto>.Fail("Active period not found."));
        var id = dto.Id.Trim();
        if (await db.BatchJobActivePeriods.AnyAsync(x => x.RecId != recId && x.DataAreaId == company && !x.IsDeleted && x.Code == id, ct))
            return Conflict(APIResponse<BatchJobActivePeriodDto>.Fail($"Active period '{id}' already exists."));
        entity.Code = id; entity.Name = Clean(dto.Name); entity.FromTimeUtc = dto.FromTimeUtc; entity.ToTimeUtc = dto.ToTimeUtc;
        entity.FromTimeLocal = dto.FromTimeLocal; entity.ToTimeLocal = dto.ToTimeLocal; entity.TimeZoneFollowed = dto.TimeZoneFollowed;
        entity.IsActive = dto.IsActive; entity.LastModifiedBy = user.GetCurrentUserId(); entity.LastModifiedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); return Ok(APIResponse<BatchJobActivePeriodDto>.Ok(Map(entity), "Active period updated."));
    }

    [HttpDelete("{recId:long}"), DomainPermission("System", "BackgroundJobs", "Delete")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var company = user.GetDataAreaId();
        var entity = await db.BatchJobActivePeriods.FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == company && !x.IsDeleted, ct);
        if (entity == null) return NotFound(APIResponse<bool>.Fail("Active period not found."));
        if (await db.SysBackgroundJobs.AnyAsync(x => !x.IsDeleted && x.ActivePeriod == entity.Code, ct))
            return Conflict(APIResponse<bool>.Fail("The active period is assigned to one or more batch jobs."));
        entity.IsDeleted = true; entity.IsActive = false; await db.SaveChangesAsync(ct);
        return Ok(APIResponse<bool>.Ok(true, "Active period deleted."));
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static BatchJobActivePeriodDto Map(SysBackgroundJobActivePeriod x) => new() { RecId = x.RecId, Id = x.Code,
        Name = x.Name, FromTimeUtc = x.FromTimeUtc, ToTimeUtc = x.ToTimeUtc, FromTimeLocal = x.FromTimeLocal,
        ToTimeLocal = x.ToTimeLocal, TimeZoneFollowed = x.TimeZoneFollowed, IsActive = x.IsActive };
}
