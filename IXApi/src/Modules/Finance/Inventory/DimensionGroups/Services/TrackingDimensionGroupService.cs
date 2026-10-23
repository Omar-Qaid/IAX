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
    private DbContext Db => _unitOfWork.Context;

    public TrackingDimensionGroupService(IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        IValidator<TrackingDimensionGroupInputDto> validator) : base(unitOfWork, currentUser) => _validator = validator;

    public async Task<List<TrackingDimensionGroupDto>> ListAsync(CancellationToken ct)
    {
        var groups = await _repository.GetQueryable().AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);
        var ids = groups.Select(x => x.RecId).ToList();
        var setup = await Db.Set<EcoResTrackingDimensionGroupFldSetup>().AsNoTracking()
            .Where(x => ids.Contains(x.TrackingDimensionGroup)).ToListAsync(ct);
        return groups.Select(x => Map(x, setup)).ToList();
    }

    public async Task<(TrackingDimensionGroupDto? Group, string? Error)> CreateAsync(TrackingDimensionGroupInputDto input, CancellationToken ct)
    {
        var error = await ValidateAsync(input, ct);
        if (error is not null) return (null, error);
        var id = NormalizeId(input.GroupId);
        if (await _repository.GetQueryable().AnyAsync(x => x.Name == id, ct))
            return (null, $"Tracking dimension group '{id}' already exists.");
        var row = input.Adapt<EcoResTrackingDimensionGroup>();
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
            throw;
        }
        return (Map(row, await LoadSetupAsync(row.RecId, ct)), null);
    }

    public async Task<(TrackingDimensionGroupDto? Group, string? Error)> UpdateAsync(long recId, TrackingDimensionGroupInputDto input, CancellationToken ct)
    {
        var row = await _repository.GetQueryable().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (row is null) return (null, "Tracking dimension group was not found.");
        var error = await ValidateAsync(input, ct);
        if (error is not null) return (null, error);
        if (row.Name != NormalizeId(input.GroupId))
            return (null, "Tracking dimension group ID cannot be changed after creation.");
        if (await Db.Set<EcoResTrackingDimensionGroupItem>().AnyAsync(x => x.TrackingDimensionGroup == recId, ct))
            return (null, "A tracking dimension group assigned to released products cannot be changed through basic setup.");
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
            throw;
        }
        return (Map(row, await LoadSetupAsync(row.RecId, ct)), null);
    }

    public async Task<(bool Deleted, string? Error)> DeleteAsync(long recId, CancellationToken ct)
    {
        var row = await _repository.GetQueryable().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (row is null) return (false, "Tracking dimension group was not found.");
        if (await _unitOfWork.Repository<EcoResTrackingDimensionGroupItem>().GetQueryable()
            .AnyAsync(x => x.TrackingDimensionGroup == row.RecId, ct))
            return (false, "The group is assigned to one or more released products.");
        Db.RemoveRange(await LoadSetupAsync(row.RecId, ct));
        await RemoveAsync(row, ct);
        return (true, null);
    }

    private async Task<string?> ValidateAsync(TrackingDimensionGroupInputDto input, CancellationToken ct) =>
        (await _validator.ValidateAsync(input, ct)).Errors.FirstOrDefault()?.ErrorMessage;
    private static string NormalizeId(string value) => value.Trim().ToUpperInvariant();

    private Task<List<EcoResTrackingDimensionGroupFldSetup>> LoadSetupAsync(long id, CancellationToken ct) =>
        Db.Set<EcoResTrackingDimensionGroupFldSetup>().Where(x => x.TrackingDimensionGroup == id).ToListAsync(ct);

    private async Task ReplaceSetupAsync(long id, TrackingDimensionGroupInputDto input, CancellationToken ct)
    {
        Db.RemoveRange(await LoadSetupAsync(id, ct));
        Db.AddRange(
            Setup(id, InventDimensionFields.BatchNumber, 1, input.UseBatchNumber),
            Setup(id, InventDimensionFields.SerialNumber, 2, input.UseSerialNumber));
        await _unitOfWork.CompleteAsync(ct);
    }

    private static EcoResTrackingDimensionGroupFldSetup Setup(long id, int fieldId, decimal line, bool active) =>
        new() { TrackingDimensionGroup = id, DimensionFieldId = fieldId, LineNumber = line, IsActive = active,
            IsPhysicalInventoryEnabled = active, IsFinancialInventoryEnabled = active,
            IsAllowBlankReceiptEnabled = !active, IsAllowBlankIssueEnabled = !active };

    private static TrackingDimensionGroupDto Map(EcoResTrackingDimensionGroup row, IEnumerable<EcoResTrackingDimensionGroupFldSetup> setup) =>
        new() { Id = row.RecId.ToString(), GroupId = row.Name, Name = row.Description,
            UseBatchNumber = setup.Any(x => x.TrackingDimensionGroup == row.RecId && x.DimensionFieldId == InventDimensionFields.BatchNumber && x.IsActive),
            UseSerialNumber = setup.Any(x => x.TrackingDimensionGroup == row.RecId && x.DimensionFieldId == InventDimensionFields.SerialNumber && x.IsActive) };
}
