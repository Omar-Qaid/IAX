using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class ItemGroupAssignmentService : BaseService<InventItemGroupItem>, IItemGroupAssignmentService
{
    private readonly ICompanyExecutionContext _company;

    public ItemGroupAssignmentService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICompanyExecutionContext company)
        : base(unitOfWork, currentUser)
    {
        _company = company;
    }

    public ItemGroupAssignmentService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
        : this(unitOfWork, NullCurrentUserService.Instance, company)
    {
    }

    public async Task<(ItemGroupAssignmentRecordDto? Assignment, string? Error)> AssignAsync(string itemId, ItemGroupAssignmentInputDto input, CancellationToken ct)
    {
        var area = _company.GetDataAreaId() ?? throw new InvalidOperationException("A company must be selected.");
        var item = (itemId ?? string.Empty).Trim().ToUpperInvariant();
        var group = (input.ItemGroupId ?? string.Empty).Trim().ToUpperInvariant();
        if (item.Length == 0 || item.Length > FieldLengths.ItemId) return (null, "A valid item ID is required.");
        if (group.Length == 0 || group.Length > FieldLengths.ItemGroupId) return (null, "A valid item group ID is required.");
        var db = _unitOfWork.Context;
        if (!await db.Set<ReleasedItem>().AnyAsync(x => x.DataAreaId == area && x.ItemId == item, ct))
            return (null, "The item must be released in the selected company.");
        if (!await db.Set<InventItemGroup>().AnyAsync(x => x.DataAreaId == area && x.ItemGroupId == group, ct))
            return (null, "The item group does not exist in the selected company.");

        var row = await db.Set<InventItemGroupItem>().FirstOrDefaultAsync(x => x.ItemId == item && x.ItemDataAreaId == area, ct);
        if (row is null)
        {
            row = new InventItemGroupItem
            {
                DataAreaId = area, ItemId = item, ItemDataAreaId = area,
                ItemGroupId = group, ItemGroupDataAreaId = area
            };
            db.Add(row);
        }
        else
        {
            if (row.DataAreaId != area || row.ItemGroupDataAreaId != area)
                return (null, "The existing item group assignment has inconsistent company references.");
            row.ItemGroupId = group;
        }
        await _unitOfWork.CompleteAsync(ct);
        return (new ItemGroupAssignmentRecordDto(row.RecId, row.ItemId, row.ItemGroupId), null);
    }
}
