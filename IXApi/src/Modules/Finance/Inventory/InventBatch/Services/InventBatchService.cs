using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventBatchService(IUnitOfWork unitOfWork, ICompanyExecutionContext company) : IInventBatchService
{
    private DbContext Db => unitOfWork.Context;

    public async Task<IReadOnlyList<InventBatchDto>> ListAsync(string? itemId, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var query = Db.Set<InventBatch>().AsNoTracking().Where(x => x.DataAreaId == companyId);
        if (!string.IsNullOrWhiteSpace(itemId))
        {
            var normalizedItem = itemId.Trim().ToUpperInvariant();
            query = query.Where(x => x.ItemId == normalizedItem);
        }
        return (await query.OrderBy(x => x.ItemId).ThenBy(x => x.InventBatchId).ToListAsync(ct))
            .Select(Map).ToList();
    }

    public async Task<InventBatchDto?> GetAsync(long recId, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var row = await Db.Set<InventBatch>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.RecId == recId, ct);
        return row is null ? null : Map(row);
    }

    public async Task<(InventBatchDto? Batch, string? Error)> CreateAsync(InventBatchInputDto input, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var itemId = Normalize(input.ItemId);
        var batchId = Normalize(input.InventBatchId);
        var error = Validate(itemId, batchId, input);
        if (error is not null) return (null, error);
        if (!await Db.Set<ReleasedItem>().AnyAsync(x => x.DataAreaId == companyId && x.ItemId == itemId, ct))
            return (null, "The item must be released in the selected company.");
        if (await Db.Set<InventBatch>().AnyAsync(x => x.DataAreaId == companyId && x.ItemId == itemId && x.InventBatchId == batchId, ct))
            return (null, $"Batch '{batchId}' already exists for item '{itemId}'.");
        var row = new InventBatch
        {
            DataAreaId = companyId, ItemId = itemId, InventBatchId = batchId,
            ProdDate = input.ProdDate?.Date ?? default, ExpDate = input.ExpDate?.Date ?? default
        };
        Db.Add(row);
        await unitOfWork.CompleteAsync(ct);
        return (Map(row), null);
    }

    public async Task<(InventBatchDto? Batch, string? Error)> UpdateAsync(long recId, InventBatchInputDto input, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var row = await Db.Set<InventBatch>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.RecId == recId, ct);
        if (row is null) return (null, "Batch was not found.");
        var itemId = Normalize(input.ItemId);
        var batchId = Normalize(input.InventBatchId);
        if (itemId != row.ItemId || batchId != row.InventBatchId)
            return (null, "Item and batch IDs cannot be changed after creation.");
        var error = Validate(itemId, batchId, input);
        if (error is not null) return (null, error);
        row.ProdDate = input.ProdDate?.Date ?? default;
        row.ExpDate = input.ExpDate?.Date ?? default;
        await unitOfWork.CompleteAsync(ct);
        return (Map(row), null);
    }

    public async Task<string?> DeleteAsync(long recId, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var row = await Db.Set<InventBatch>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.RecId == recId, ct);
        if (row is null) return "Batch was not found.";
        var usedByTransaction = await Db.Set<InventTrans>().AnyAsync(trans =>
            trans.DataAreaId == companyId && trans.ItemId == row.ItemId
            && Db.Set<InventDim>().Any(dim => dim.DataAreaId == companyId
                && dim.InventDimId == trans.InventDimId && dim.InventBatchId == row.InventBatchId), ct);
        if (usedByTransaction || await Db.Set<InventSum>().AnyAsync(x => x.DataAreaId == companyId
            && x.ItemId == row.ItemId && x.InventBatchId == row.InventBatchId, ct))
            return "Batch is referenced by inventory transactions or balances and cannot be deleted.";
        Db.Remove(row);
        await unitOfWork.CompleteAsync(ct);
        return null;
    }

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();

    private static string? Validate(string itemId, string batchId, InventBatchInputDto input) =>
        itemId.Length == 0 ? "Item is required." :
        itemId.Length > FieldLengths.ItemId ? $"Item ID cannot exceed {FieldLengths.ItemId} characters." :
        batchId.Length == 0 ? "Batch ID is required." :
        batchId.Length > FieldLengths.InventBatchId ? $"Batch ID cannot exceed {FieldLengths.InventBatchId} characters." :
        input.ProdDate is not null && input.ExpDate is not null && input.ExpDate.Value.Date < input.ProdDate.Value.Date
            ? "Expiration date cannot precede production date." : null;

    private static InventBatchDto Map(InventBatch row) => new()
    {
        RecId = row.RecId, ItemId = row.ItemId, InventBatchId = row.InventBatchId,
        ProdDate = row.ProdDate == default ? null : row.ProdDate,
        ExpDate = row.ExpDate == default ? null : row.ExpDate
    };

    private string RequireCompany() => company.GetDataAreaId()
        ?? throw new InvalidOperationException("A company must be selected.");
}
