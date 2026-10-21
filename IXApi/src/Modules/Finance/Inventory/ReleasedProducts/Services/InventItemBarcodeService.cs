using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventItemBarcodeService(IUnitOfWork unitOfWork, ICompanyExecutionContext company) : IInventItemBarcodeService
{
    private DbContext Db => unitOfWork.Context;

    public async Task<(List<InventItemBarcodeDto>? Rows, string? Error)> ListAsync(long releasedProductRecId, CancellationToken ct)
    {
        var item = await ItemAsync(releasedProductRecId, ct);
        if (item is null) return (null, "The released product was not found in the selected company.");
        var rows = await Db.Set<InventItemBarcode>().AsNoTracking()
            .Where(x => x.DataAreaId == item.DataAreaId && x.ItemId == item.ItemId)
            .OrderBy(x => x.ItemBarcode).ThenBy(x => x.RecId).ToListAsync(ct);
        return (rows.Select(Map).ToList(), null);
    }

    public async Task<(InventItemBarcodeDto? Row, string? Error)> CreateAsync(long releasedProductRecId, InventItemBarcodeInputDto input, CancellationToken ct)
    {
        var item = await ItemAsync(releasedProductRecId, ct);
        if (item is null) return (null, "The released product was not found in the selected company.");
        var error = await ValidateAsync(item, input, 0, ct);
        if (error is not null) return (null, error);
        var row = new InventItemBarcode { DataAreaId = item.DataAreaId, ItemId = item.ItemId };
        Apply(row, input);
        Db.Add(row);
        await unitOfWork.CompleteAsync(ct);
        return (Map(row), null);
    }

    public async Task<(InventItemBarcodeDto? Row, string? Error)> UpdateAsync(long releasedProductRecId, long barcodeRecId, InventItemBarcodeInputDto input, CancellationToken ct)
    {
        var item = await ItemAsync(releasedProductRecId, ct);
        if (item is null) return (null, "The released product was not found in the selected company.");
        var row = await Db.Set<InventItemBarcode>().FirstOrDefaultAsync(
            x => x.RecId == barcodeRecId && x.DataAreaId == item.DataAreaId && x.ItemId == item.ItemId, ct);
        if (row is null) return (null, "The barcode was not found for this released product.");
        var error = await ValidateAsync(item, input, row.RecId, ct);
        if (error is not null) return (null, error);
        Apply(row, input);
        await unitOfWork.CompleteAsync(ct);
        return (Map(row), null);
    }

    public async Task<(bool Deleted, string? Error)> DeleteAsync(long releasedProductRecId, long barcodeRecId, CancellationToken ct)
    {
        var item = await ItemAsync(releasedProductRecId, ct);
        if (item is null) return (false, "The released product was not found in the selected company.");
        var row = await Db.Set<InventItemBarcode>().FirstOrDefaultAsync(
            x => x.RecId == barcodeRecId && x.DataAreaId == item.DataAreaId && x.ItemId == item.ItemId, ct);
        if (row is null) return (false, "The barcode was not found for this released product.");
        Db.Remove(row);
        await unitOfWork.CompleteAsync(ct);
        return (true, null);
    }

    private Task<ReleasedItem?> ItemAsync(long recId, CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        return string.IsNullOrWhiteSpace(companyId)
            ? Task.FromResult<ReleasedItem?>(null)
            : Db.Set<ReleasedItem>().AsNoTracking().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == companyId, ct);
    }

    private async Task<string?> ValidateAsync(ReleasedItem item, InventItemBarcodeInputDto input, long existingRecId, CancellationToken ct)
    {
        if (input.ItemBarcode is null || input.UnitId is null || input.Description is null || input.InventDimId is null)
            return "Barcode, unit, description, and dimension fields must be provided.";
        var code = input.ItemBarcode.Trim();
        var unit = input.UnitId.Trim();
        var dimension = input.InventDimId.Trim();
        if (code.Length == 0 || code.Length > FieldLengths.ItemBarcode) return "Barcode is required and must fit its configured length.";
        if (unit.Length == 0 || unit.Length > FieldLengths.UnitId) return "A valid unit of measure is required.";
        if (input.Description.Trim().Length > FieldLengths.Description || dimension.Length > FieldLengths.InventDimId)
            return "Barcode description or inventory dimension exceeds its allowed length.";
        if (input.Qty <= 0) return "Barcode quantity must be greater than zero.";
        if (!await Db.Set<UnitOfMeasure>().AnyAsync(x => x.Symbol == unit, ct)) return "The selected unit of measure does not exist.";
        if (dimension.Length > 0 && !await Db.Set<InventDim>().AnyAsync(x => x.DataAreaId == item.DataAreaId && x.InventDimId == dimension, ct))
            return "The selected inventory dimension does not exist in this company.";
        if (await Db.Set<InventItemBarcode>().AnyAsync(x => x.DataAreaId == item.DataAreaId &&
            x.ItemId == item.ItemId && x.ItemBarcode == code && x.UnitId == unit &&
            x.InventDimId == dimension && x.RecId != existingRecId, ct))
            return "The barcode association already exists for this item, unit, and dimension.";
        return null;
    }

    private static void Apply(InventItemBarcode row, InventItemBarcodeInputDto input)
    {
        row.ItemBarcode = input.ItemBarcode.Trim();
        row.UnitId = input.UnitId.Trim();
        row.Qty = input.Qty;
        row.Description = input.Description.Trim();
        row.InventDimId = input.InventDimId.Trim();
        row.UseForInput = input.UseForInput ? NoYes.Yes : NoYes.No;
        row.UseForPrinting = input.UseForPrinting ? NoYes.Yes : NoYes.No;
        row.Blocked = input.Blocked ? NoYes.Yes : NoYes.No;
    }

    private static InventItemBarcodeDto Map(InventItemBarcode row) => new(
        row.RecId, row.ItemBarcode, row.ItemId, row.UnitId, row.Qty, row.Description, row.InventDimId,
        row.UseForInput == NoYes.Yes, row.UseForPrinting == NoYes.Yes, row.Blocked == NoYes.Yes);
}
