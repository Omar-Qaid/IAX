using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services;

public sealed class SysBackgroundJobGroupService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    : ISysBackgroundJobGroupService
{
    public async Task<List<SysBackgroundJobGroupDto>> GetAsync(CancellationToken cancellationToken)
    {
        var company = currentUser.GetDataAreaId();
        var rows = await unitOfWork.Context.Set<SysBackgroundJobGroup>().AsNoTracking()
            .Where(group => group.DataAreaId == company && !group.IsDeleted)
            .OrderBy(group => group.GroupCode).ToListAsync(cancellationToken);
        return rows.Adapt<List<SysBackgroundJobGroupDto>>();
    }

    public async Task<BatchSetupResult<SysBackgroundJobGroupDto>> CreateAsync(
        SysBackgroundJobGroupDto dto, CancellationToken cancellationToken)
    {
        var company = currentUser.GetDataAreaId();
        var code = dto.GroupCode.Trim();
        var set = unitOfWork.Context.Set<SysBackgroundJobGroup>();
        if (await set.AnyAsync(group => group.DataAreaId == company && !group.IsDeleted &&
            group.GroupCode == code, cancellationToken))
            return new(BatchSetupStatus.Conflict, Error: $"Batch group '{code}' already exists.");
        var entity = dto.Adapt<SysBackgroundJobGroup>();
        entity.GroupCode = code;
        entity.Description = Clean(dto.Description);
        entity.DataAreaId = company;
        entity.CreatedBy = currentUser.GetCurrentUserId();
        entity.CreatedAt = DateTime.UtcNow;
        set.Add(entity);
        await unitOfWork.Context.SaveChangesAsync(cancellationToken);
        return new(BatchSetupStatus.Ok, entity.Adapt<SysBackgroundJobGroupDto>());
    }

    public async Task<BatchSetupResult<SysBackgroundJobGroupDto>> UpdateAsync(
        long id, SysBackgroundJobGroupDto dto, CancellationToken cancellationToken)
    {
        var company = currentUser.GetDataAreaId();
        var set = unitOfWork.Context.Set<SysBackgroundJobGroup>();
        var entity = await set.FirstOrDefaultAsync(group => group.RecId == id && group.DataAreaId == company &&
            !group.IsDeleted, cancellationToken);
        if (entity == null) return new(BatchSetupStatus.NotFound, Error: "Batch group not found.");
        var code = dto.GroupCode.Trim();
        if (await set.AnyAsync(group => group.RecId != id && group.DataAreaId == company &&
            !group.IsDeleted && group.GroupCode == code, cancellationToken))
            return new(BatchSetupStatus.Conflict, Error: $"Batch group '{code}' already exists.");
        dto.Adapt(entity);
        entity.GroupCode = code;
        entity.Description = Clean(dto.Description);
        entity.LastModifiedBy = currentUser.GetCurrentUserId();
        entity.LastModifiedAt = DateTime.UtcNow;
        entity.RecVersion++;
        await unitOfWork.Context.SaveChangesAsync(cancellationToken);
        return new(BatchSetupStatus.Ok, entity.Adapt<SysBackgroundJobGroupDto>());
    }

    public async Task<BatchSetupResult<bool>> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var company = currentUser.GetDataAreaId();
        var entity = await unitOfWork.Context.Set<SysBackgroundJobGroup>()
            .FirstOrDefaultAsync(group => group.RecId == id && group.DataAreaId == company && !group.IsDeleted,
                cancellationToken);
        if (entity == null) return new(BatchSetupStatus.NotFound, Error: "Batch group not found.");
        if (await unitOfWork.Context.Set<SysBackgroundJob>().AnyAsync(job => !job.IsDeleted &&
            job.BatchGroup == entity.GroupCode, cancellationToken))
            return new(BatchSetupStatus.Conflict, Error: "The batch group is assigned to one or more batch jobs.");
        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.LastModifiedAt = DateTime.UtcNow;
        entity.LastModifiedBy = currentUser.GetCurrentUserId();
        await unitOfWork.Context.SaveChangesAsync(cancellationToken);
        return new(BatchSetupStatus.Ok, true);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
