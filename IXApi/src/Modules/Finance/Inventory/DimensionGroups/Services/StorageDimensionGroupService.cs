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
    private DbContext Db => _unitOfWork.Context;

    public StorageDimensionGroupService(IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        IValidator<StorageDimensionGroupInputDto> validator) : base(unitOfWork, currentUser) => _validator = validator;

    public async Task<List<StorageDimensionGroupDto>> ListAsync(CancellationToken ct)
    {
        var groups = await _repository.GetQueryable().AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);
        var ids = groups.Select(x => x.RecId).ToList();
        var setup = await Db.Set<EcoResStorageDimensionGroupFldSetup>().AsNoTracking()
            .Where(x => ids.Contains(x.StorageDimensionGroup)).ToListAsync(ct);
        return groups.Select(x => Map(x, setup)).ToList();
    }

    public Task<(StorageDimensionGroupDto? Group, string? Error)> CreateAsync(StorageDimensionGroupInputDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => CreateCoreAsync(input, ct));

    private async Task<(StorageDimensionGroupDto? Group, string? Error)> CreateCoreAsync(StorageDimensionGroupInputDto input, CancellationToken ct)
    {
        var error = await ValidateAsync(input, ct);
        if (error is not null) return (null, error);
        var id = NormalizeId(input.GroupId);
        if (await _repository.GetQueryable().AnyAsync(x => x.Name == id, ct))
            return (null, $"Storage dimension group '{id}' already exists.");
        var row = input.Adapt<EcoResStorageDimensionGroup>();
        row.Name = id;
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await AddAsync(row, ct);
            await ReplaceSetupAsync(row.RecId, input, ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
        return (Map(row, await LoadSetupAsync(row.RecId, ct)), null);
    }

    public Task<(StorageDimensionGroupDto? Group, string? Error)> UpdateAsync(long recId, StorageDimensionGroupInputDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => UpdateCoreAsync(recId, input, ct));

    private async Task<(StorageDimensionGroupDto? Group, string? Error)> UpdateCoreAsync(long recId, StorageDimensionGroupInputDto input, CancellationToken ct)
    {
        var row = await _repository.GetQueryable().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (row is null) return (null, "Storage dimension group was not found.");
        var error = await ValidateAsync(input, ct);
        if (error is not null) return (null, error);
        if (row.Name != NormalizeId(input.GroupId))
            return (null, "Storage dimension group ID cannot be changed after creation.");
        if (await Db.Set<EcoResStorageDimensionGroupItem>().AnyAsync(x => x.StorageDimensionGroup == recId, ct))
            return (null, "A storage dimension group assigned to released products cannot be changed through basic setup.");
        input.Adapt(row);
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await base.UpdateAsync(row, ct);
            await ReplaceSetupAsync(row.RecId, input, ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
        return (Map(row, await LoadSetupAsync(row.RecId, ct)), null);
    }

    public async Task<(bool Deleted, string? Error)> DeleteAsync(long recId, CancellationToken ct)
    {
        var row = await _repository.GetQueryable().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (row is null) return (false, "Storage dimension group was not found.");
        if (await _unitOfWork.Repository<EcoResStorageDimensionGroupItem>().GetQueryable()
            .AnyAsync(x => x.StorageDimensionGroup == row.RecId, ct))
            return (false, "The group is assigned to one or more released products.");
        Db.RemoveRange(await LoadSetupAsync(row.RecId, ct));
        await RemoveAsync(row, ct);
        return (true, null);
    }

    private async Task<string?> ValidateAsync(StorageDimensionGroupInputDto input, CancellationToken ct) =>
        (await _validator.ValidateAsync(input, ct)).Errors.FirstOrDefault()?.ErrorMessage;
    private static string NormalizeId(string value) => value.Trim().ToUpperInvariant();

    private Task<List<EcoResStorageDimensionGroupFldSetup>> LoadSetupAsync(long id, CancellationToken ct) =>
        Db.Set<EcoResStorageDimensionGroupFldSetup>().Where(x => x.StorageDimensionGroup == id).ToListAsync(ct);

    private async Task ReplaceSetupAsync(long id, StorageDimensionGroupInputDto input, CancellationToken ct)
    {
        Db.RemoveRange(await LoadSetupAsync(id, ct));
        Db.AddRange(
            Setup(id, InventDimensionFields.Site, 1, input.UseSite, true),
            Setup(id, InventDimensionFields.Warehouse, 2, input.UseWarehouse, false),
            Setup(id, InventDimensionFields.Location, 3, input.UseLocation, false));
        await _unitOfWork.CompleteAsync(ct);
    }

    private static EcoResStorageDimensionGroupFldSetup Setup(long id, int fieldId, decimal line, bool active, bool financial) =>
        new() { StorageDimensionGroup = id, DimensionFieldId = fieldId, LineNumber = line, IsActive = active,
            IsPhysicalInventoryEnabled = active, IsFinancialInventoryEnabled = active && financial,
            IsAllowBlankReceiptEnabled = !active, IsAllowBlankIssueEnabled = !active };

    private static StorageDimensionGroupDto Map(EcoResStorageDimensionGroup row, IEnumerable<EcoResStorageDimensionGroupFldSetup> setup) =>
        new() { Id = row.RecId.ToString(), GroupId = row.Name, Name = row.Description,
            UseSite = setup.Any(x => x.StorageDimensionGroup == row.RecId && x.DimensionFieldId == InventDimensionFields.Site && x.IsActive),
            UseWarehouse = setup.Any(x => x.StorageDimensionGroup == row.RecId && x.DimensionFieldId == InventDimensionFields.Warehouse && x.IsActive),
            UseLocation = setup.Any(x => x.StorageDimensionGroup == row.RecId && x.DimensionFieldId == InventDimensionFields.Location && x.IsActive) };
}
