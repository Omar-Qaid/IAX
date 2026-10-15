using FluentValidation;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class TrackingDimensionGroupService : BaseService<EcoResTrackingDimensionGroup>, ITrackingDimensionGroupService
{
    private readonly IValidator<TrackingDimensionGroupInputDto> _validator;

    public TrackingDimensionGroupService(IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        IValidator<TrackingDimensionGroupInputDto> validator) : base(unitOfWork, currentUser) => _validator = validator;

    public async Task<List<TrackingDimensionGroupDto>> ListAsync(CancellationToken ct) =>
        (await _repository.GetQueryable().AsNoTracking().OrderBy(x => x.TrackingDimensionGroupId).ToListAsync(ct))
        .Adapt<List<TrackingDimensionGroupDto>>();

    public async Task<(TrackingDimensionGroupDto? Group, string? Error)> CreateAsync(TrackingDimensionGroupInputDto input, CancellationToken ct)
    {
        var error = await ValidateAsync(input, ct);
        if (error is not null) return (null, error);
        var id = NormalizeId(input.GroupId);
        if (await _repository.GetQueryable().AnyAsync(x => x.TrackingDimensionGroupId == id, ct))
            return (null, $"Tracking dimension group '{id}' already exists.");
        var row = input.Adapt<EcoResTrackingDimensionGroup>();
        row.TrackingDimensionGroupId = id;
        await AddAsync(row, ct);
        return (row.Adapt<TrackingDimensionGroupDto>(), null);
    }

    public async Task<(TrackingDimensionGroupDto? Group, string? Error)> UpdateAsync(long recId, TrackingDimensionGroupInputDto input, CancellationToken ct)
    {
        var row = await _repository.GetQueryable().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (row is null) return (null, "Tracking dimension group was not found.");
        var error = await ValidateAsync(input, ct);
        if (error is not null) return (null, error);
        if (row.TrackingDimensionGroupId != NormalizeId(input.GroupId))
            return (null, "Tracking dimension group ID cannot be changed after creation.");
        input.Adapt(row);
        await base.UpdateAsync(row, ct);
        return (row.Adapt<TrackingDimensionGroupDto>(), null);
    }

    public async Task<(bool Deleted, string? Error)> DeleteAsync(long recId, CancellationToken ct)
    {
        var row = await _repository.GetQueryable().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (row is null) return (false, "Tracking dimension group was not found.");
        if (await _unitOfWork.Repository<EcoResTrackingDimensionGroupItem>().GetQueryable()
            .AnyAsync(x => x.TrackingDimensionGroupId == row.TrackingDimensionGroupId, ct))
            return (false, "The group is assigned to one or more released products.");
        await RemoveAsync(row, ct);
        return (true, null);
    }

    private async Task<string?> ValidateAsync(TrackingDimensionGroupInputDto input, CancellationToken ct) =>
        (await _validator.ValidateAsync(input, ct)).Errors.FirstOrDefault()?.ErrorMessage;
    private static string NormalizeId(string value) => value.Trim().ToUpperInvariant();
}
