using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;

public sealed class ProductLifecycleService(IUnitOfWork unitOfWork, ICompanyExecutionContext company) : IProductLifecycleService
{
    private DbContext Db => unitOfWork.Context;

    public async Task<List<ProductLifecycleStateDto>> ListAsync(CancellationToken ct) =>
        await Db.Set<EcoResProductLifecycleState>().AsNoTracking().OrderBy(x => x.StateId)
            .Select(x => new ProductLifecycleStateDto(x.RecId, x.StateId, x.Description,
                x.IsDefaultOnProductRelease == NoYes.Yes, x.BlockSales == NoYes.Yes,
                x.BlockPurchase == NoYes.Yes, x.BlockInventory == NoYes.Yes)).ToListAsync(ct);

    public async Task<(ProductLifecycleStateDto? Row, string? Error)> SaveAsync(
        long? recId, ProductLifecycleStateInputDto input, CancellationToken ct)
    {
        var stateId = input.StateId?.Trim() ?? string.Empty;
        var description = input.Description?.Trim() ?? string.Empty;
        if (stateId.Length == 0 || stateId.Length > FieldLengths.Name || description.Length > FieldLengths.Description)
            return (null, "Status ID is required and status fields must fit their allowed lengths.");
        var row = recId.HasValue
            ? await Db.Set<EcoResProductLifecycleState>().FirstOrDefaultAsync(x => x.RecId == recId.Value, ct)
            : new EcoResProductLifecycleState();
        if (row is null) return (null, "Product status was not found.");
        if (recId.HasValue && row.StateId != stateId)
            return (null, "Status ID cannot be changed after creation.");
        if (await Db.Set<EcoResProductLifecycleState>().AnyAsync(x => x.StateId == stateId && x.RecId != row.RecId, ct))
            return (null, "Product status already exists.");
        if (input.IsDefaultOnProductRelease && await Db.Set<EcoResProductLifecycleState>()
                .AnyAsync(x => x.IsDefaultOnProductRelease == NoYes.Yes && x.RecId != row.RecId, ct))
            return (null, "Only one product status can be the default for release.");
        if (!recId.HasValue) Db.Add(row);
        row.StateId = stateId;
        row.Description = description;
        row.IsDefaultOnProductRelease = input.IsDefaultOnProductRelease ? NoYes.Yes : NoYes.No;
        row.BlockSales = input.BlockSales ? NoYes.Yes : NoYes.No;
        row.BlockPurchase = input.BlockPurchase ? NoYes.Yes : NoYes.No;
        row.BlockInventory = input.BlockInventory ? NoYes.Yes : NoYes.No;
        await unitOfWork.CompleteAsync(ct);
        return (new(row.RecId, row.StateId, row.Description, input.IsDefaultOnProductRelease,
            input.BlockSales, input.BlockPurchase, input.BlockInventory), null);
    }

    public async Task<string?> DeleteAsync(long recId, CancellationToken ct)
    {
        var row = await Db.Set<EcoResProductLifecycleState>().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (row is null) return "Product status was not found.";
        if (await Db.Set<ReleasedItem>().AnyAsync(x => x.ProductLifecycleStateId == row.StateId, ct))
            return "A product status assigned to released products cannot be deleted.";
        Db.Remove(row);
        await unitOfWork.CompleteAsync(ct);
        return null;
    }

    public async Task<(ReleasedProductLifecycleDto? Item, string? Error)> AssignAsync(
        long releasedProductRecId, string stateId, CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        var item = await Db.Set<ReleasedItem>().FirstOrDefaultAsync(
            x => x.RecId == releasedProductRecId && x.DataAreaId == companyId, ct);
        if (item is null) return (null, "Released product was not found in the selected company.");
        var normalizedStateId = stateId?.Trim() ?? string.Empty;
        if (normalizedStateId.Length > 0 && !await Db.Set<EcoResProductLifecycleState>()
                .AnyAsync(x => x.StateId == normalizedStateId, ct))
            return (null, "Product status was not found.");
        item.ProductLifecycleStateId = normalizedStateId;
        await unitOfWork.CompleteAsync(ct);
        return (new(item.RecId, item.ItemId, item.ProductLifecycleStateId), null);
    }
}
