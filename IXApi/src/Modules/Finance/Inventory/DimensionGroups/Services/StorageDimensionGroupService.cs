using FluentValidation;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class StorageDimensionGroupService : BaseService<EcoResStorageDimensionGroup>, IStorageDimensionGroupService
{
    private readonly IValidator<StorageDimensionGroupInputDto> _validator;

    public StorageDimensionGroupService(IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        IValidator<StorageDimensionGroupInputDto> validator) : base(unitOfWork, currentUser) => _validator = validator;

    public async Task<List<StorageDimensionGroupDto>> ListAsync(CancellationToken ct) =>
        (await _repository.GetQueryable().AsNoTracking().OrderBy(x => x.StorageDimensionGroupId).ToListAsync(ct))
        .Adapt<List<StorageDimensionGroupDto>>();

    public async Task<(StorageDimensionGroupDto? Group, string? Error)> CreateAsync(StorageDimensionGroupInputDto input, CancellationToken ct)
    {
        var error = await ValidateAsync(input, ct);
        if (error is not null) return (null, error);
        var id = NormalizeId(input.GroupId);
        if (await _repository.GetQueryable().AnyAsync(x => x.StorageDimensionGroupId == id, ct))
            return (null, $"Storage dimension group '{id}' already exists.");
        var row = input.Adapt<EcoResStorageDimensionGroup>();
        row.StorageDimensionGroupId = id;
        await AddAsync(row, ct);
        return (row.Adapt<StorageDimensionGroupDto>(), null);
    }

    public async Task<(StorageDimensionGroupDto? Group, string? Error)> UpdateAsync(long recId, StorageDimensionGroupInputDto input, CancellationToken ct)
    {
        var row = await _repository.GetQueryable().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (row is null) return (null, "Storage dimension group was not found.");
        var error = await ValidateAsync(input, ct);
        if (error is not null) return (null, error);
        if (row.StorageDimensionGroupId != NormalizeId(input.GroupId))
            return (null, "Storage dimension group ID cannot be changed after creation.");
        input.Adapt(row);
        await base.UpdateAsync(row, ct);
        return (row.Adapt<StorageDimensionGroupDto>(), null);
    }

    public async Task<(bool Deleted, string? Error)> DeleteAsync(long recId, CancellationToken ct)
    {
        var row = await _repository.GetQueryable().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (row is null) return (false, "Storage dimension group was not found.");
        if (await _unitOfWork.Repository<EcoResStorageDimensionGroupItem>().GetQueryable()
            .AnyAsync(x => x.StorageDimensionGroupId == row.StorageDimensionGroupId, ct))
            return (false, "The group is assigned to one or more released products.");
        await RemoveAsync(row, ct);
        return (true, null);
    }

    private async Task<string?> ValidateAsync(StorageDimensionGroupInputDto input, CancellationToken ct) =>
        (await _validator.ValidateAsync(input, ct)).Errors.FirstOrDefault()?.ErrorMessage;
    private static string NormalizeId(string value) => value.Trim().ToUpperInvariant();
}
