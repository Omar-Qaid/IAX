using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using PostingRecord = IAX.IXApi.Modules.Finance.Entities.InventTransPosting;

namespace IAX.IXApi.Modules.Finance.Inventory.InventTransPosting;

/// <summary>
/// Reads inventory posting records, and records them for the posting services. Posting records
/// are produced by posting operations only; nothing here writes one outside a posting.
/// </summary>
public sealed class InventTransPostingService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    : IInventTransPostingService
{
    private DbContext Db => unitOfWork.Context;

    public async Task<(IReadOnlyList<InventTransPostingDto>? Items, string? Error)> ByInventTransIdAsync(string inventTransId, CancellationToken ct)
    {
        var area = currentUser.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return (null, "A company must be selected.");
        var id = (inventTransId ?? string.Empty).Trim();
        if (id.Length == 0) return (null, "An inventory transaction identifier is required.");
        var origins = Db.Set<InventTransOrigin>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.InventTransId == id).Select(x => x.RecId);
        return (await QueryAsync(area, x => origins.Contains(x.InventTransOrigin), ct), null);
    }

    public async Task<(IReadOnlyList<InventTransPostingDto>? Items, string? Error)> ByVoucherAsync(string voucher, CancellationToken ct)
    {
        var area = currentUser.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return (null, "A company must be selected.");
        var id = (voucher ?? string.Empty).Trim();
        if (id.Length == 0) return (null, "A voucher is required.");
        return (await QueryAsync(area, x => x.Voucher == id, ct), null);
    }

    private async Task<List<InventTransPostingDto>> QueryAsync(string area,
        System.Linq.Expressions.Expression<Func<PostingRecord, bool>> filter, CancellationToken ct) =>
        await (from posting in Db.Set<PostingRecord>().AsNoTracking().Where(x => x.DataAreaId == area).Where(filter)
               join origin in Db.Set<InventTransOrigin>().AsNoTracking() on posting.InventTransOrigin equals origin.RecId
               orderby posting.TransDate, posting.RecId
               select new InventTransPostingDto(posting.RecId, posting.InventTransOrigin,
                   origin.InventTransId, posting.ItemId, posting.InventTransPostingType,
                   posting.IsPosted, posting.LedgerDimension, posting.OffsetLedgerDimension,
                   posting.PostingType, posting.PostingTypeOffset, posting.Voucher, posting.TransDate))
            .ToListAsync(ct);

    /// <summary>
    /// Records what an inventory posting sent to the ledger. Called by posting services inside
    /// their own transaction, right after the voucher's account entries are built.
    /// </summary>
    internal static PostingRecord Record(string area, long inventTransOrigin, string itemId,
        InventTransPostingType type, DateTime transDate, string voucher,
        DimensionAttributeValueCombination account, LedgerPostingType postingType,
        DimensionAttributeValueCombination offset, LedgerPostingType offsetPostingType) => new()
        {
            DataAreaId = area,
            InventTransOrigin = inventTransOrigin,
            ItemId = itemId,
            InventTransPostingType = type,
            IsPosted = NoYes.Yes,
            TransDate = transDate,
            TransBeginTime = DateTime.UtcNow,
            Voucher = voucher,
            LedgerDimension = account.RecId,
            PostingType = postingType,
            OffsetLedgerDimension = offset.RecId,
            PostingTypeOffset = offsetPostingType,
            ProjId = string.Empty
        };
}
