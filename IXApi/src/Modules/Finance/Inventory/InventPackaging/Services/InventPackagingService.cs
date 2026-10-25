using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventPackagingService : BaseService<InventPackagingGroup>, IInventPackagingService
{
    private readonly ICompanyExecutionContext _company;

    public InventPackagingService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICompanyExecutionContext company)
        : base(unitOfWork, currentUser)
    {
        _company = company;
    }

    public InventPackagingService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
        : this(unitOfWork, new NullCurrentUserService(company), company)
    {
    }

    private DbContext Db => _unitOfWork.Context;
    private string Area => _company.GetDataAreaId() ?? throw new InvalidOperationException("A company must be selected.");
    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();
    private static InventPackagingGroupRecordDto Map(InventPackagingGroup row) =>
        new(row.RecId, row.PackagingGroupId, row.Name);

    public async Task<IReadOnlyList<InventPackagingGroupRecordDto>> GroupsAsync(CancellationToken ct)
    {
        var area = Area;
        return (await Db.Set<InventPackagingGroup>().AsNoTracking().Where(x => x.DataAreaId == area)
            .OrderBy(x => x.PackagingGroupId).ToListAsync(ct)).Select(Map).ToList();
    }

    public async Task<IReadOnlyList<InventPackagingUnitRecordDto>> UnitsAsync(CancellationToken ct)
    {
        var area = Area;
        return await Db.Set<InventPackagingUnit>().AsNoTracking().Where(x => x.DataAreaId == area)
            .OrderBy(x => x.RecId).Take(200)
            .Select(x => new InventPackagingUnitRecordDto(x.RecId, x.ItemCode, x.InventDimId, x.Unit, x.Factor))
            .ToListAsync(ct);
    }

    public async Task<(InventPackagingGroupRecordDto? Group, string? Error)> CreateGroupAsync(InventPackagingGroupInputDto input, CancellationToken ct)
    {
        var area = Area;
        var id = Normalize(input.PackagingGroupId);
        var name = (input.Name ?? string.Empty).Trim();
        var error = Validate(id, name);
        if (error is not null) return (null, error);
        if (await Db.Set<InventPackagingGroup>().AnyAsync(x => x.DataAreaId == area && x.PackagingGroupId == id, ct))
            return (null, $"Packaging group '{id}' already exists.");
        var row = new InventPackagingGroup { DataAreaId = area, PackagingGroupId = id, Name = name };
        Db.Add(row);
        await _unitOfWork.CompleteAsync(ct);
        return (Map(row), null);
    }

    public async Task<(InventPackagingGroupRecordDto? Group, string? Error)> UpdateGroupAsync(long recId, InventPackagingGroupInputDto input, CancellationToken ct)
    {
        var area = Area;
        var row = await Db.Set<InventPackagingGroup>().FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
        if (row is null) return (null, "Packaging group was not found.");
        var id = Normalize(input.PackagingGroupId);
        var name = (input.Name ?? string.Empty).Trim();
        if (id != row.PackagingGroupId) return (null, "Packaging group ID cannot be changed after creation.");
        var error = Validate(id, name);
        if (error is not null) return (null, error);
        row.Name = name;
        await _unitOfWork.CompleteAsync(ct);
        return (Map(row), null);
    }

    public async Task<string?> DeleteGroupAsync(long recId, CancellationToken ct)
    {
        var area = Area;
        var row = await Db.Set<InventPackagingGroup>().FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
        if (row is null) return "Packaging group was not found.";
        Db.Remove(row);
        await _unitOfWork.CompleteAsync(ct);
        return null;
    }

    private static string? Validate(string id, string name) =>
        id.Length == 0 ? "Packaging group ID is required." :
        id.Length > FieldLengths.PackagingGroupId ? $"Packaging group ID cannot exceed {FieldLengths.PackagingGroupId} characters." :
        name.Length == 0 ? "Packaging group name is required." :
        name.Length > FieldLengths.Name ? $"Packaging group name cannot exceed {FieldLengths.Name} characters." : null;

    private sealed class NullCurrentUserService(ICompanyExecutionContext company) : ICurrentUserService
    {
        public string GetCurrentUserId() => string.Empty;
        public string GetOwnerAccountId() => string.Empty;
        public string GetDataAreaId() => company.GetDataAreaId();
    }
}
