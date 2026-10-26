using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services;

public sealed class BatchJobActivePeriodService(IUnitOfWork unitOfWork, ICurrentUserService user)
    : BaseService<SysBackgroundJobActivePeriod>(unitOfWork, user), IBatchJobActivePeriodService
{
    public async Task<List<BatchJobActivePeriodDto>> GetAsync(CancellationToken cancellationToken)
    {
        var company = user.GetDataAreaId();
        var entities = await unitOfWork.Context.Set<SysBackgroundJobActivePeriod>().AsNoTracking()
            .Where(period => period.DataAreaId == company && !period.IsDeleted)
            .OrderBy(period => period.Code).ToListAsync(cancellationToken);
        return entities.Adapt<List<BatchJobActivePeriodDto>>();
    }

    public async Task<BatchSetupResult<BatchJobActivePeriodDto>> CreateAsync(
        BatchJobActivePeriodDto dto, CancellationToken cancellationToken)
    {
        var company = user.GetDataAreaId();
        var code = dto.Id.Trim();
        var set = unitOfWork.Context.Set<SysBackgroundJobActivePeriod>();
        if (await set.AnyAsync(period => period.DataAreaId == company && !period.IsDeleted && period.Code == code,
            cancellationToken))
            return new(BatchSetupStatus.Conflict, Error: $"Active period '{code}' already exists.");
        var entity = dto.Adapt<SysBackgroundJobActivePeriod>();
        entity.Code = code;
        entity.Name = Clean(dto.Name);
        entity.DataAreaId = company;
        entity.CreatedBy = user.GetCurrentUserId();
        entity.CreatedAt = DateTime.UtcNow;
        set.Add(entity);
        await unitOfWork.Context.SaveChangesAsync(cancellationToken);
        return new(BatchSetupStatus.Ok, entity.Adapt<BatchJobActivePeriodDto>());
    }

    public async Task<BatchSetupResult<BatchJobActivePeriodDto>> UpdateAsync(
        long recId, BatchJobActivePeriodDto dto, CancellationToken cancellationToken)
    {
        var company = user.GetDataAreaId();
        var set = unitOfWork.Context.Set<SysBackgroundJobActivePeriod>();
        var entity = await set.FirstOrDefaultAsync(period => period.RecId == recId &&
            period.DataAreaId == company && !period.IsDeleted, cancellationToken);
        if (entity == null) return new(BatchSetupStatus.NotFound, Error: "Active period not found.");
        var code = dto.Id.Trim();
        if (await set.AnyAsync(period => period.RecId != recId && period.DataAreaId == company &&
            !period.IsDeleted && period.Code == code, cancellationToken))
            return new(BatchSetupStatus.Conflict, Error: $"Active period '{code}' already exists.");
        dto.Adapt(entity);
        entity.Code = code;
        entity.Name = Clean(dto.Name);
        entity.LastModifiedBy = user.GetCurrentUserId();
        entity.LastModifiedAt = DateTime.UtcNow;
        await unitOfWork.Context.SaveChangesAsync(cancellationToken);
        return new(BatchSetupStatus.Ok, entity.Adapt<BatchJobActivePeriodDto>());
    }

    public async Task<BatchSetupResult<bool>> DeleteAsync(long recId, CancellationToken cancellationToken)
    {
        var company = user.GetDataAreaId();
        var entity = await unitOfWork.Context.Set<SysBackgroundJobActivePeriod>()
            .FirstOrDefaultAsync(period => period.RecId == recId && period.DataAreaId == company && !period.IsDeleted,
                cancellationToken);
        if (entity == null) return new(BatchSetupStatus.NotFound, Error: "Active period not found.");
        if (await unitOfWork.Context.Set<SysBackgroundJob>().AnyAsync(job => !job.IsDeleted &&
            job.ActivePeriod == entity.Code, cancellationToken))
            return new(BatchSetupStatus.Conflict, Error: "The active period is assigned to one or more batch jobs.");
        entity.IsDeleted = true;
        entity.IsActive = false;
        await unitOfWork.Context.SaveChangesAsync(cancellationToken);
        return new(BatchSetupStatus.Ok, true);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
