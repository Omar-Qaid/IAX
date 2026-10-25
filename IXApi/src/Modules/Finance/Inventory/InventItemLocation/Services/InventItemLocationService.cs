using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventItemLocationService : BaseService<InventItemLocation>, IInventItemLocationService
{
    private readonly ICompanyExecutionContext _company;
    private DbContext Db => _unitOfWork.Context;
    private string Area => _company.GetDataAreaId() ?? throw new InvalidOperationException("A company must be selected.");
    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();

    public InventItemLocationService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICompanyExecutionContext company)
        : base(unitOfWork, currentUser)
    {
        _company = company;
    }

    public InventItemLocationService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
        : this(unitOfWork, NullCurrentUserService.Instance, company)
    {
    }

    public async Task<(ItemLocationRecordDto? Location, string? Error)> CreateAsync(InventItemLocationInputDto input, CancellationToken ct)
    {
        var area = Area;
        var item = Normalize(input.ItemId);
        var dimId = Normalize(input.InventDimId);
        var error = await ValidateAsync(area, item, dimId, input, ct);
        if (error is not null) return (null, error);
        if (await Db.Set<InventItemLocation>().AnyAsync(x => x.DataAreaId == area && x.ItemId == item && x.InventDimId == dimId, ct))
            return (null, "An item location already exists for this item and dimension.");
        var row = new InventItemLocation { DataAreaId = area, ItemId = item, InventDimId = dimId,
            PickingLocationMaxQty = input.PickingLocationMaxQty, PickingLocationRefillMin = input.PickingLocationRefillMin,
            CountGroupId = Normalize(input.CountGroupId) };
        Db.Add(row);
        await _unitOfWork.CompleteAsync(ct);
        return (Map(row), null);
    }

    public async Task<(ItemLocationRecordDto? Location, string? Error)> UpdateAsync(long recId, InventItemLocationInputDto input, CancellationToken ct)
    {
        var area = Area;
        var row = await Db.Set<InventItemLocation>().FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
        if (row is null) return (null, "Item location was not found.");
        var item = Normalize(input.ItemId);
        var dimId = Normalize(input.InventDimId);
        if (item != row.ItemId || dimId != row.InventDimId)
            return (null, "Item and dimension IDs cannot be changed after creation.");
        var error = await ValidateAsync(area, item, dimId, input, ct);
        if (error is not null) return (null, error);
        row.PickingLocationMaxQty = input.PickingLocationMaxQty;
        row.PickingLocationRefillMin = input.PickingLocationRefillMin;
        if (input.CountGroupId is not null) row.CountGroupId = Normalize(input.CountGroupId);
        await _unitOfWork.CompleteAsync(ct);
        return (Map(row), null);
    }

    public async Task<string?> DeleteAsync(long recId, CancellationToken ct)
    {
        var area = Area;
        var row = await Db.Set<InventItemLocation>().FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
        if (row is null) return "Item location was not found.";
        Db.Remove(row);
        await _unitOfWork.CompleteAsync(ct);
        return null;
    }

    private async Task<string?> ValidateAsync(string area, string item, string dimId, InventItemLocationInputDto input, CancellationToken ct)
    {
        if (item.Length == 0 || item.Length > FieldLengths.ItemId) return "A valid item ID is required.";
        if (dimId.Length == 0 || dimId.Length > FieldLengths.InventDimId) return "A valid inventory dimension ID is required.";
        if (input.PickingLocationMaxQty < 0 || input.PickingLocationRefillMin < 0)
            return "Picking quantities cannot be negative.";
        if (input.PickingLocationMaxQty > 0 && input.PickingLocationRefillMin > input.PickingLocationMaxQty)
            return "Refill minimum cannot exceed the picking maximum.";
        var countGroup = Normalize(input.CountGroupId);
        if (countGroup.Length > FieldLengths.CountGroupId) return "Count group ID is too long.";
        if (countGroup.Length > 0 && !await Db.Set<InventCountGroup>()
            .AnyAsync(x => x.DataAreaId == area && x.CountGroupId == countGroup, ct))
            return "Count group does not exist in the selected company.";
        if (!await Db.Set<ReleasedItem>().AnyAsync(x => x.DataAreaId == area && x.ItemId == item, ct))
            return "The item must be released in the selected company.";
        var dim = await Db.Set<InventDim>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == area && x.InventDimId == dimId, ct);
        if (dim is null || dim.InventSiteId.Length == 0 || dim.InventLocationId.Length == 0
            || dim.InventBatchId.Length > 0 || dim.InventSerialId.Length > 0 || dim.InventStatusId.Length > 0
            || dim.WmsLocationId.Length > 0 || dim.LicensePlateId.Length > 0
            || dim.ConfigId.Length > 0 || dim.InventSizeId.Length > 0 || dim.InventColorId.Length > 0
            || dim.InventStyleId.Length > 0 || dim.InventVersionId.Length > 0)
            return "The dimension must contain only a site and warehouse in the selected company.";
        if (!await Db.Set<InventLocation>().AnyAsync(x => x.DataAreaId == area
            && x.InventLocationId == dim.InventLocationId && x.InventSiteId == dim.InventSiteId, ct))
            return "The warehouse does not belong to the dimension's site in the selected company.";
        return null;
    }

    private static ItemLocationRecordDto Map(InventItemLocation row) => new(row.RecId, row.ItemId, row.InventDimId,
        row.PickingLocationMaxQty, row.PickingLocationRefillMin, row.CountGroupId,
        row.WmsLocationIdDefaultReceipt, row.WmsLocationIdDefaultIssue);
}
