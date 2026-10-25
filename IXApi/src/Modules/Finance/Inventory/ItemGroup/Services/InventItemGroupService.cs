using System.Globalization;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using Mapster;
using InventItemGroupEntity = IAX.IXApi.Modules.Finance.Entities.InventItemGroup;
using InventItemGroupItemEntity = IAX.IXApi.Modules.Finance.Entities.InventItemGroupItem;
using InventPostingEntity = IAX.IXApi.Modules.Finance.Entities.InventPosting;
using LedgerDimensionEntity = IAX.IXApi.Modules.Finance.Entities.DimensionAttributeValueCombination;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventItemGroupService : BaseService<InventItemGroupEntity>, IInventItemGroupService
{
    private readonly ICompanyExecutionContext company;
    private DbContext Db => _unitOfWork.Context;

    public InventItemGroupService(IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        ICompanyExecutionContext company) : base(unitOfWork, currentUser) => this.company = company;

    public InventItemGroupService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
        : this(unitOfWork, new NullCurrentUserService(company), company)
    {
    }

    private sealed class NullCurrentUserService(ICompanyExecutionContext company) : ICurrentUserService
    {
        public string GetCurrentUserId() => string.Empty;
        public string GetOwnerAccountId() => string.Empty;
        public string GetDataAreaId() => company.GetDataAreaId();
    }

    public async Task<List<InventItemGroupRecordDto>?> ListAsync(CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;
        var groups = await Db.Set<InventItemGroupEntity>().AsNoTracking().Where(x => x.DataAreaId == companyId)
            .OrderBy(x => x.ItemGroupId).ToListAsync(ct);
        var relations = groups.Select(x => x.ItemGroupId).ToList();
        var postings = await Db.Set<InventPostingEntity>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.ItemCode == TableGroupAll.Group && relations.Contains(x.ItemRelation))
            .ToListAsync(ct);
        var dimensionIds = postings.Select(x => x.LedgerDimension).Where(x => x != 0).Distinct().ToList();
        var accounts = await Db.Set<LedgerDimensionEntity>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && dimensionIds.Contains(x.RecId))
            .ToDictionaryAsync(x => x.RecId, x => x.MainAccountValue, ct);
        return groups.Select(group => Map(group, postings.Where(x => x.ItemRelation == group.ItemGroupId), accounts)).ToList();
    }

    public async Task<List<InventItemGroupPostingDto>?> LedgerAccountsAsync(CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;
        return await Db.Set<LedgerDimensionEntity>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.MainAccountValue != string.Empty)
            .OrderBy(x => x.MainAccountValue).Select(x => new InventItemGroupPostingDto
            {
                Id = x.RecId.ToString(), LedgerDimension = x.RecId.ToString(CultureInfo.InvariantCulture), MainAccount = x.MainAccountValue
            }).ToListAsync(ct);
    }

    public Task<(InventItemGroupRecordDto? Group, string? Error)> CreateAsync(InventItemGroupRecordDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => CreateCoreAsync(input, ct));

    private async Task<(InventItemGroupRecordDto? Group, string? Error)> CreateCoreAsync(InventItemGroupRecordDto input, CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A legal entity must be selected.");
        var id = input.ItemGroupId.Trim().ToUpperInvariant();
        var error = Validate(id, input);
        if (error is not null) return (null, error);
        if (await Db.Set<InventItemGroupEntity>().AnyAsync(x => x.DataAreaId == companyId && x.ItemGroupId == id, ct))
            return (null, $"Item group '{id}' already exists.");
        error = await ValidatePostingsAsync(companyId, input.Postings, ct);
        if (error is not null) return (null, error);
        var row = new InventItemGroupEntity { ItemGroupId = id, Name = input.Name.Trim(), DataAreaId = companyId };
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            Db.Add(row);
            await _unitOfWork.CompleteAsync(ct);
            error = await SyncPostingsAsync(companyId, id, input.Postings, ct);
            if (error is not null)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                Db.ChangeTracker.Clear();
                return (null, error);
            }
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
        return await LoadOneAsync(row, companyId, ct);
    }

    public Task<(InventItemGroupRecordDto? Group, string? Error)> UpdateAsync(long recId, InventItemGroupRecordDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => UpdateCoreAsync(recId, input, ct));

    private async Task<(InventItemGroupRecordDto? Group, string? Error)> UpdateCoreAsync(long recId, InventItemGroupRecordDto input, CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        var row = await Db.Set<InventItemGroupEntity>().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == companyId, ct);
        if (row is null) return (null, "Item group was not found.");
        var id = input.ItemGroupId.Trim().ToUpperInvariant();
        var error = Validate(id, input);
        if (error is not null) return (null, error);
        if (row.ItemGroupId != id) return (null, "Item group ID cannot be changed after creation.");
        error = await ValidatePostingsAsync(companyId!, input.Postings, ct);
        if (error is not null) return (null, error);
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            row.Name = input.Name.Trim();
            error = await SyncPostingsAsync(companyId!, id, input.Postings, ct);
            if (error is not null)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                Db.ChangeTracker.Clear();
                return (null, error);
            }
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
        return await LoadOneAsync(row, companyId!, ct);
    }

    public async Task<(bool Deleted, string? Error)> DeleteAsync(long recId, CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        var row = await Db.Set<InventItemGroupEntity>().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == companyId, ct);
        if (row is null) return (false, "Item group was not found.");
        if (await Db.Set<InventItemGroupItemEntity>().AnyAsync(x => x.ItemGroupDataAreaId == companyId && x.ItemGroupId == row.ItemGroupId, ct))
            return (false, "The group is assigned to one or more released products.");
        if (await Db.Set<InventPostingEntity>().AnyAsync(x => x.DataAreaId == companyId && x.ItemCode == TableGroupAll.Group && x.ItemRelation == row.ItemGroupId, ct))
            return (false, "The group is referenced by inventory posting setup.");
        Db.Remove(row); await _unitOfWork.CompleteAsync(ct); return (true, null);
    }

    private async Task<string?> SyncPostingsAsync(string companyId, string itemGroupId, List<InventItemGroupPostingDto> input, CancellationToken ct)
    {
        var validationError = await ValidatePostingsAsync(companyId, input, ct);
        if (validationError is not null) return validationError;
        var accountTypes = input.Select(x => x.AccountType).Distinct().ToList();
        var parsed = new List<(InventItemGroupPostingDto Posting, long Dimension)>();
        foreach (var posting in input)
        {
            if (!long.TryParse(posting.LedgerDimension, NumberStyles.None, CultureInfo.InvariantCulture, out var dimension) || dimension < 0)
                return "A ledger dimension identifier is invalid.";
            parsed.Add((posting, dimension));
        }
        var existing = await Db.Set<InventPostingEntity>().Where(x => x.DataAreaId == companyId && x.ItemCode == TableGroupAll.Group && x.ItemRelation == itemGroupId && accountTypes.Contains((int)x.InventAccountType)).ToListAsync(ct);
        foreach (var entry in parsed)
        {
            var posting = entry.Posting;
            var row = existing.FirstOrDefault(x => (int)x.InventAccountType == posting.AccountType);
            if (entry.Dimension == 0) { if (row is not null) Db.Remove(row); continue; }
            if (row is null)
            {
                row = new InventPostingEntity { DataAreaId = companyId, ItemCode = TableGroupAll.Group, ItemRelation = itemGroupId, CustVendCode = TableGroupAll.All, CustVendRelation = string.Empty, InventAccountType = (InventAccountType)posting.AccountType };
                Db.Add(row);
            }
            row.LedgerDimension = entry.Dimension;
        }
        await _unitOfWork.CompleteAsync(ct);
        return null;
    }

    private async Task<string?> ValidatePostingsAsync(string companyId, List<InventItemGroupPostingDto> input, CancellationToken ct)
    {
        if (input.Select(x => x.AccountType).Distinct().Count() != input.Count || input.Any(x => !Enum.IsDefined(typeof(InventAccountType), x.AccountType)))
            return "An inventory account type is invalid or duplicated.";
        var dimensionIds = new List<long>();
        foreach (var posting in input)
        {
            if (!long.TryParse(posting.LedgerDimension, NumberStyles.None, CultureInfo.InvariantCulture, out var dimension) || dimension < 0)
                return "A ledger dimension identifier is invalid.";
            if (dimension != 0) dimensionIds.Add(dimension);
        }
        dimensionIds = dimensionIds.Distinct().ToList();
        var validCount = await Db.Set<LedgerDimensionEntity>().AsNoTracking()
            .CountAsync(x => x.DataAreaId == companyId && dimensionIds.Contains(x.RecId), ct);
        return validCount == dimensionIds.Count ? null : "One or more main accounts are invalid for the selected legal entity.";
    }

    private async Task<(InventItemGroupRecordDto? Group, string? Error)> LoadOneAsync(InventItemGroupEntity row, string companyId, CancellationToken ct)
    {
        var postings = await Db.Set<InventPostingEntity>().AsNoTracking().Where(x => x.DataAreaId == companyId && x.ItemCode == TableGroupAll.Group && x.ItemRelation == row.ItemGroupId).ToListAsync(ct);
        var ids = postings.Select(x => x.LedgerDimension).Where(x => x != 0).Distinct().ToList();
        var accounts = await Db.Set<LedgerDimensionEntity>().AsNoTracking().Where(x => x.DataAreaId == companyId && ids.Contains(x.RecId)).ToDictionaryAsync(x => x.RecId, x => x.MainAccountValue, ct);
        return (Map(row, postings, accounts), null);
    }

    private static string? Validate(string id, InventItemGroupRecordDto input) => id.Length == 0 ? "Item group is required." : string.IsNullOrWhiteSpace(input.Name) ? "Name is required." : null;
    private static InventItemGroupRecordDto Map(
        InventItemGroupEntity group,
        IEnumerable<InventPostingEntity> postings,
        IReadOnlyDictionary<long, string> accounts)
    {
        var postingDtos = postings
            .Select(row => new InventItemGroupPostingMappingSource(
                row.RecId.ToString(CultureInfo.InvariantCulture),
                (int)row.InventAccountType,
                row.LedgerDimension,
                accounts.GetValueOrDefault(row.LedgerDimension, string.Empty)))
            .Adapt<List<InventItemGroupPostingDto>>();

        return new InventItemGroupMappingSource(group, postingDtos).Adapt<InventItemGroupRecordDto>();
    }
}
