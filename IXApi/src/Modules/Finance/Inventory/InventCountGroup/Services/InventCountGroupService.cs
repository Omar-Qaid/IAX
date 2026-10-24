using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventCountGroupService(IUnitOfWork unitOfWork, ICompanyExecutionContext company) : IInventCountGroupService
{
    private DbContext Db => unitOfWork.Context;
    private string Area => company.GetDataAreaId() ?? throw new InvalidOperationException("A company must be selected.");
    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();
    private static InventCountGroupRecordDto Map(InventCountGroup row) =>
        new(row.RecId, row.CountGroupId, row.Name, row.CountCode, row.CountPeriod);

    public async Task<IReadOnlyList<InventCountGroupRecordDto>> ListAsync(CancellationToken ct)
    {
        var area = Area;
        return (await Db.Set<InventCountGroup>().AsNoTracking().Where(x => x.DataAreaId == area)
            .OrderBy(x => x.CountGroupId).Take(200).ToListAsync(ct)).Select(Map).ToList();
    }

    public async Task<InventCountGroupRecordDto?> GetAsync(long recId, CancellationToken ct)
    {
        var area = Area;
        var row = await Db.Set<InventCountGroup>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
        return row is null ? null : Map(row);
    }

    public async Task<(InventCountGroupRecordDto? Group, string? Error)> CreateAsync(InventCountGroupInputDto input, CancellationToken ct)
    {
        var area = Area;
        var id = Normalize(input.CountGroupId);
        var name = (input.Name ?? string.Empty).Trim();
        var error = Validate(id, name, input);
        if (error is not null) return (null, error);
        if (await Db.Set<InventCountGroup>().AnyAsync(x => x.DataAreaId == area && x.CountGroupId == id, ct))
            return (null, "Count group already exists in the selected company.");
        var row = new InventCountGroup { DataAreaId = area, CountGroupId = id,
            Name = name, CountCode = input.CountCode, CountPeriod = input.CountPeriod };
        Db.Add(row);
        await unitOfWork.CompleteAsync(ct);
        return (Map(row), null);
    }

    public async Task<(InventCountGroupRecordDto? Group, string? Error)> UpdateAsync(long recId, InventCountGroupInputDto input, CancellationToken ct)
    {
        var area = Area;
        var row = await Db.Set<InventCountGroup>().FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
        if (row is null) return (null, "Count group was not found.");
        var id = Normalize(input.CountGroupId);
        if (id != row.CountGroupId) return (null, "Count group ID cannot be changed after creation.");
        var name = (input.Name ?? string.Empty).Trim();
        var error = Validate(id, name, input);
        if (error is not null) return (null, error);
        row.Name = name;
        row.CountCode = input.CountCode;
        row.CountPeriod = input.CountPeriod;
        await unitOfWork.CompleteAsync(ct);
        return (Map(row), null);
    }

    public async Task<string?> DeleteAsync(long recId, CancellationToken ct)
    {
        var area = Area;
        var row = await Db.Set<InventCountGroup>().FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
        if (row is null) return "Count group was not found.";
        if (await Db.Set<InventItemLocation>().AnyAsync(x => x.DataAreaId == area && x.CountGroupId == row.CountGroupId, ct))
            return "Count group is assigned to an item location and cannot be deleted.";
        Db.Remove(row);
        await unitOfWork.CompleteAsync(ct);
        return null;
    }

    private static string? Validate(string id, string name, InventCountGroupInputDto input) =>
        id.Length == 0 || id.Length > FieldLengths.CountGroupId ? "A valid count group ID is required." :
        name.Length == 0 || name.Length > FieldLengths.Name ? "A valid count group name is required." :
        !Enum.IsDefined(input.CountCode) ? "Count code is invalid." :
        input.CountCode == InventCountCode.Period && input.CountPeriod <= 0 ? "Period counting requires a positive interval in days." :
        input.CountCode != InventCountCode.Period && input.CountPeriod != 0 ? "Count period is only valid for Period counting." : null;
}
