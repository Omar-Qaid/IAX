using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventSerialRecordDto(long RecId, string ItemId, string InventSerialId, DateTime? ProdDate);
public sealed record InventSerialInputDto(string ItemId, string InventSerialId, DateTime? ProdDate);

public interface IInventSerialService
{
    Task<IReadOnlyList<InventSerialRecordDto>> ListAsync(string? itemId, CancellationToken ct);
    Task<(InventSerialRecordDto? Serial, string? Error)> CreateAsync(InventSerialInputDto input, CancellationToken ct);
    Task<(InventSerialRecordDto? Serial, string? Error)> UpdateAsync(long recId, InventSerialInputDto input, CancellationToken ct);
    Task<string?> DeleteAsync(long recId, CancellationToken ct);
}

public sealed class InventSerialService(IUnitOfWork unitOfWork, ICompanyExecutionContext company) : IInventSerialService
{
    private DbContext Db => unitOfWork.Context;
    private string Area => company.GetDataAreaId() ?? throw new InvalidOperationException("A company must be selected.");
    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();
    private static InventSerialRecordDto Map(InventSerial row) => new(row.RecId, row.ItemId,
        row.InventSerialId, row.ProdDate == default ? null : row.ProdDate);

    public async Task<IReadOnlyList<InventSerialRecordDto>> ListAsync(string? itemId, CancellationToken ct)
    {
        var area = Area;
        var item = Normalize(itemId);
        var query = Db.Set<InventSerial>().AsNoTracking().Where(x => x.DataAreaId == area);
        if (item.Length > 0) query = query.Where(x => x.ItemId == item);
        return (await query.OrderBy(x => x.ItemId).ThenBy(x => x.InventSerialId).Take(200).ToListAsync(ct))
            .Select(Map).ToList();
    }

    public async Task<(InventSerialRecordDto? Serial, string? Error)> CreateAsync(InventSerialInputDto input, CancellationToken ct)
    {
        var area = Area;
        var item = Normalize(input.ItemId);
        var serial = Normalize(input.InventSerialId);
        var error = Validate(item, serial);
        if (error is not null) return (null, error);
        if (!await Db.Set<ReleasedItem>().AnyAsync(x => x.DataAreaId == area && x.ItemId == item, ct))
            return (null, "The item must be released in the selected company.");
        if (await Db.Set<InventSerial>().AnyAsync(x => x.DataAreaId == area && x.ItemId == item && x.InventSerialId == serial, ct))
            return (null, $"Serial '{serial}' already exists for item '{item}'.");
        var row = new InventSerial { DataAreaId = area, ItemId = item, InventSerialId = serial,
            ProdDate = input.ProdDate?.Date ?? default };
        Db.Add(row);
        await unitOfWork.CompleteAsync(ct);
        return (Map(row), null);
    }

    public async Task<(InventSerialRecordDto? Serial, string? Error)> UpdateAsync(long recId, InventSerialInputDto input, CancellationToken ct)
    {
        var area = Area;
        var row = await Db.Set<InventSerial>().FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
        if (row is null) return (null, "Serial was not found.");
        if (Normalize(input.ItemId) != row.ItemId || Normalize(input.InventSerialId) != row.InventSerialId)
            return (null, "Item and serial IDs cannot be changed after creation.");
        row.ProdDate = input.ProdDate?.Date ?? default;
        await unitOfWork.CompleteAsync(ct);
        return (Map(row), null);
    }

    public async Task<string?> DeleteAsync(long recId, CancellationToken ct)
    {
        var area = Area;
        var row = await Db.Set<InventSerial>().FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
        if (row is null) return "Serial was not found.";
        if (await Db.Set<InventDim>().AnyAsync(x => x.DataAreaId == area && x.InventSerialId == row.InventSerialId, ct)
            || await Db.Set<InventSum>().AnyAsync(x => x.DataAreaId == area && x.ItemId == row.ItemId && x.InventSerialId == row.InventSerialId, ct))
            return "Serial is referenced by inventory dimensions or balances and cannot be deleted.";
        Db.Remove(row);
        await unitOfWork.CompleteAsync(ct);
        return null;
    }

    private static string? Validate(string item, string serial) =>
        item.Length == 0 ? "Item is required." :
        item.Length > FieldLengths.ItemId ? $"Item ID cannot exceed {FieldLengths.ItemId} characters." :
        serial.Length == 0 ? "Serial ID is required." :
        serial.Length > FieldLengths.InventSerialId ? $"Serial ID cannot exceed {FieldLengths.InventSerialId} characters." : null;
}
