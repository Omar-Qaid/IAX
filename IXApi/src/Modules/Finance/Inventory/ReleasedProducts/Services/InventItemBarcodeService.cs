using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventItemBarcodeService : BaseService<InventItemBarcode>, IInventItemBarcodeService
{
    private readonly ICompanyExecutionContext _company;
    private DbContext Db => _unitOfWork.Context;

    public InventItemBarcodeService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICompanyExecutionContext company)
        : base(unitOfWork, currentUser)
    {
        _company = company;
    }

    public InventItemBarcodeService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
        : this(unitOfWork, NullCurrentUserService.Instance, company)
    {
    }

    public async Task<(List<InventItemBarcodeDto>? Rows, string? Error)> ListAsync(long releasedProductRecId, CancellationToken ct)
    {
        var item = await ItemAsync(releasedProductRecId, ct);
        if (item is null) return (null, "The released product was not found in the selected company.");
        var rows = await Db.Set<InventItemBarcode>().AsNoTracking()
            .Where(x => x.DataAreaId == item.DataAreaId && x.ItemId == item.ItemId)
            .OrderBy(x => x.ItemBarcode).ThenBy(x => x.RecId).ToListAsync(ct);
        return (rows.Select(Map).ToList(), null);
    }

    public async Task<(List<InventItemBarcodeLookupDto>? Rows, string? Error)> FindAsync(string barcode, CancellationToken ct)
    {
        var companyId = _company.GetDataAreaId();
        var code = barcode?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        if (code.Length == 0 || code.Length > FieldLengths.ItemBarcode)
            return (null, "A valid barcode is required.");
        var rows = await (from row in Db.Set<InventItemBarcode>().AsNoTracking()
                          join item in Db.Set<ReleasedItem>().AsNoTracking()
                              on new { row.DataAreaId, row.ItemId } equals new { item.DataAreaId, item.ItemId }
                          where row.DataAreaId == companyId && row.ItemBarcode == code &&
                                row.Blocked == NoYes.No && row.UseForInput == NoYes.Yes
                          orderby item.ItemId, row.UnitId
                          select new { ItemRecId = item.RecId, Barcode = row }).ToListAsync(ct);
        if (rows.Count > 1) return (null, "The barcode resolves to more than one released item or unit in this company.");
        if (rows.Count == 0) return ([], null);
        var selected = rows[0];
        var releasedItem = await ItemAsync(selected.ItemRecId, ct);
        if (releasedItem is null) return (null, "The barcode does not reference a released item in this company.");
        var converted = await ConvertToInventoryUnitAsync(releasedItem, selected.Barcode.UnitId, selected.Barcode.Qty, ct);
        if (converted is null) return (null, "The barcode unit has no supported conversion to the item's inventory unit.");
        return ([new InventItemBarcodeLookupDto(selected.ItemRecId, Map(selected.Barcode), converted.Value.Unit, converted.Value.Quantity)], null);
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
        await _unitOfWork.CompleteAsync(ct);
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
        await _unitOfWork.CompleteAsync(ct);
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
        await _unitOfWork.CompleteAsync(ct);
        return (true, null);
    }

    private Task<ReleasedItem?> ItemAsync(long recId, CancellationToken ct)
    {
        var companyId = _company.GetDataAreaId();
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
        if (await ConvertToInventoryUnitAsync(item, unit, input.Qty, ct) is null)
            return "The barcode unit has no supported conversion to the item's inventory unit.";
        if (dimension.Length > 0 && !await Db.Set<InventDim>().AnyAsync(x => x.DataAreaId == item.DataAreaId && x.InventDimId == dimension, ct))
            return "The selected inventory dimension does not exist in this company.";
        if (await Db.Set<InventItemBarcode>().AnyAsync(x => x.DataAreaId == item.DataAreaId &&
            x.ItemBarcode == code && x.RecId != existingRecId, ct))
            return "The barcode already maps to a released item or unit in this company.";
        return null;
    }

    private async Task<(string Unit, decimal Quantity)?> ConvertToInventoryUnitAsync(ReleasedItem item, string fromUnit, decimal quantity, CancellationToken ct)
    {
        var inventoryUnit = await Db.Set<InventTableModule>().AsNoTracking()
            .Where(x => x.DataAreaId == item.DataAreaId && x.ItemId == item.ItemId && x.ModuleType == ModuleInventPurchSales.Inventory)
            .Select(x => x.UnitId).FirstOrDefaultAsync(ct);
        if (string.IsNullOrWhiteSpace(inventoryUnit)) return null;
        if (string.Equals(fromUnit, inventoryUnit, StringComparison.OrdinalIgnoreCase)) return (inventoryUnit, quantity);
        var units = await Db.Set<UnitOfMeasure>().AsNoTracking()
            .Where(x => x.Symbol == fromUnit || x.Symbol == inventoryUnit).ToListAsync(ct);
        var from = units.FirstOrDefault(x => x.Symbol == fromUnit);
        var to = units.FirstOrDefault(x => x.Symbol == inventoryUnit);
        if (from is null || to is null) return null;
        var conversion = await Db.Set<UnitOfMeasureConversion>().AsNoTracking()
            .Where(x => (x.Product == item.Product || x.Product == 0)
                && (x.FromUnitOfMeasure == from.RecId && x.ToUnitOfMeasure == to.RecId
                    || x.FromUnitOfMeasure == to.RecId && x.ToUnitOfMeasure == from.RecId))
            .OrderByDescending(x => x.Product == item.Product)
            .ThenByDescending(x => x.FromUnitOfMeasure == from.RecId)
            .ThenByDescending(x => x.RecId).FirstOrDefaultAsync(ct);
        if (conversion is null) return null;
        var factor = conversion.Factor > 0 ? conversion.Factor :
            conversion.Numerator > 0 && conversion.Denominator > 0
                ? (decimal)conversion.Numerator / conversion.Denominator : 0m;
        if (factor <= 0 || conversion.InnerOffset != 0 || conversion.OuterOffset != 0) return null;
        var converted = conversion.FromUnitOfMeasure == from.RecId
            ? quantity * factor : quantity / factor;
        var scale = (decimal)Math.Pow(10, to.DecimalPrecision);
        converted = conversion.Rounding switch
        {
            UnitOfMeasureRounding.Up => Math.Ceiling(converted * scale) / scale,
            UnitOfMeasureRounding.Down => Math.Floor(converted * scale) / scale,
            _ => Math.Round(converted, to.DecimalPrecision, MidpointRounding.AwayFromZero)
        };
        return converted > 0 ? (inventoryUnit, converted) : null;
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
