using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory.InventoryStatus;

public sealed class InventoryStatusService : BaseService<WHSInventStatus>, IInventoryStatusService
{
    private readonly ICompanyExecutionContext _company;

    public InventoryStatusService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICompanyExecutionContext company)
        : base(unitOfWork, currentUser)
    {
        _company = company;
    }

    public InventoryStatusService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
        : this(unitOfWork, new NullCurrentUserService(company), company)
    {
    }

    private DbContext Db => _unitOfWork.Context;

    public async Task<List<InventoryStatusDto>?> ListAsync(CancellationToken ct)
    {
        var companyId = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;
        return await Db.Set<WHSInventStatus>().AsNoTracking().Where(x => x.DataAreaId == companyId)
            .OrderBy(x => x.InventStatusId)
            .Select(x => new InventoryStatusDto(x.RecId, x.InventStatusId, x.Name,
                x.InventStatusBlocking == NoYes.Yes)).ToListAsync(ct);
    }

    public async Task<(InventoryStatusDto? Row, string? Error)> SaveAsync(long? recId, InventoryStatusInputDto input, CancellationToken ct)
    {
        var companyId = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        var statusId = input.InventStatusId?.Trim().ToUpperInvariant() ?? string.Empty;
        var name = input.Name?.Trim() ?? string.Empty;
        if (statusId.Length == 0 || statusId.Length > FieldLengths.InventStatusId ||
            name.Length == 0 || name.Length > FieldLengths.Name)
            return (null, "Inventory status ID and name are required and must fit their allowed lengths.");
        var row = recId.HasValue
            ? await Db.Set<WHSInventStatus>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.RecId == recId.Value, ct)
            : new WHSInventStatus { DataAreaId = companyId };
        if (row is null) return (null, "Inventory status was not found in the selected company.");
        if (recId.HasValue && row.InventStatusId != statusId)
            return (null, "Inventory status ID cannot be changed after creation.");
        if (await Db.Set<WHSInventStatus>().AnyAsync(x => x.DataAreaId == companyId && x.InventStatusId == statusId && x.RecId != row.RecId, ct))
            return (null, "Inventory status already exists in the selected company.");
        if (!recId.HasValue) Db.Add(row);
        row.InventStatusId = statusId;
        row.Name = name;
        row.InventStatusBlocking = input.Blocked ? NoYes.Yes : NoYes.No;
        await _unitOfWork.CompleteAsync(ct);
        return (new(row.RecId, row.InventStatusId, row.Name, input.Blocked), null);
    }

    public async Task<string?> DeleteAsync(long recId, CancellationToken ct)
    {
        var companyId = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return "A company must be selected.";
        var row = await Db.Set<WHSInventStatus>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.RecId == recId, ct);
        if (row is null) return "Inventory status was not found in the selected company.";
        if (await Db.Set<InventDim>().AnyAsync(x => x.DataAreaId == companyId && x.InventStatusId == row.InventStatusId, ct) ||
            await Db.Set<InventSum>().AnyAsync(x => x.DataAreaId == companyId && x.InventStatusId == row.InventStatusId, ct) ||
            await Db.Set<InventSite>().AnyAsync(x => x.DataAreaId == companyId && x.DefaultInventStatusID == row.InventStatusId, ct))
            return "An inventory status used by a dimension or site cannot be deleted.";
        Db.Remove(row);
        await _unitOfWork.CompleteAsync(ct);
        return null;
    }

    private sealed class NullCurrentUserService(ICompanyExecutionContext company) : ICurrentUserService
    {
        public string GetCurrentUserId() => string.Empty;
        public string GetOwnerAccountId() => string.Empty;
        public string GetDataAreaId() => company.GetDataAreaId();
    }
}
