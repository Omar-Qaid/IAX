using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

/// <summary>
/// Inventory marking: tying an issue to the specific receipt its cost must come from, so cost
/// settlement uses that receipt instead of the inventory model's order.
///
/// D365 keeps no separate marking table. The link lives on the inventory transaction itself, in
/// <see cref="InventTrans.MarkingRefInventTransOrigin"/>, which references the counterpart's
/// <see cref="InventTransOrigin"/>, and marking is always written in both directions. This service
/// therefore adds no schema: it maintains that standard field on both sides.
///
/// A marking reference is one field per transaction, so D365 marks part of a quantity by splitting
/// the transaction. This service marks whole transactions and says so, rather than inventing a
/// marked-quantity column. Cost settlement consumes the marked receipt first and lets any
/// remaining quantity fall through to the inventory model.
/// </summary>
public sealed class InventoryMarkingService(
    IUnitOfWork unitOfWork, ICompanyExecutionContext company) : IInventoryMarkingService
{
    private DbContext Db => unitOfWork.Context;

    public Task<(InventoryMarkingDto? Result, string? Error)> MarkAsync(string issueInventTransId,
        string receiptInventTransId, CancellationToken ct = default) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() =>
            ApplyAsync(issueInventTransId, receiptInventTransId, ct));

    public Task<(InventoryMarkingDto? Result, string? Error)> UnmarkAsync(string inventTransId,
        CancellationToken ct = default) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => ClearAsync(inventTransId, ct));

    private async Task<(InventoryMarkingDto? Result, string? Error)> ApplyAsync(string issueInventTransId,
        string receiptInventTransId, CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        var issueId = (issueInventTransId ?? string.Empty).Trim();
        var receiptId = (receiptInventTransId ?? string.Empty).Trim();
        if (issueId.Length == 0 || receiptId.Length == 0)
            return (null, "An issue and a receipt inventory transaction are both required.");
        if (string.Equals(issueId, receiptId, StringComparison.OrdinalIgnoreCase))
            return (null, "An inventory transaction cannot be marked to itself.");

        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var (issueOrigin, issue, issueError) = await LoadSideAsync(companyId, issueId, ct);
            if (issueError != null) return await Fail(issueError, ct);
            var (receiptOrigin, receipt, receiptError) = await LoadSideAsync(companyId, receiptId, ct);
            if (receiptError != null) return await Fail(receiptError, ct);

            if (issue!.Qty >= 0) return await Fail($"Inventory transaction '{issueId}' is not an issue.", ct);
            if (receipt!.Qty <= 0) return await Fail($"Inventory transaction '{receiptId}' is not a receipt.", ct);
            if (!string.Equals(issue.ItemId, receipt.ItemId, StringComparison.Ordinal))
                return await Fail("An issue can only be marked to a receipt of the same item.", ct);
            if (issue.ValueOpen == InventTransOpen.No || receipt.ValueOpen == InventTransOpen.No)
                return await Fail("A financially settled transaction cannot be marked.", ct);

            // Marking the same pair again is a no-operation; marking over a different link is not.
            if (issue.MarkingRefInventTransOrigin == receiptOrigin!.RecId &&
                receipt.MarkingRefInventTransOrigin == issueOrigin!.RecId)
            {
                await unitOfWork.CommitTransactionAsync(ct);
                return (Map(issue, issueId, receiptId), null);
            }
            if (issue.MarkingRefInventTransOrigin != 0)
                return await Fail($"Inventory transaction '{issueId}' is already marked. Remove the marking first.", ct);
            if (receipt.MarkingRefInventTransOrigin != 0)
                return await Fail($"Inventory transaction '{receiptId}' is already marked. Remove the marking first.", ct);

            issue.MarkingRefInventTransOrigin = receiptOrigin.RecId;
            receipt.MarkingRefInventTransOrigin = issueOrigin!.RecId;
            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return (Map(issue, issueId, receiptId), null);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    private async Task<(InventoryMarkingDto? Result, string? Error)> ClearAsync(string inventTransId, CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        var id = (inventTransId ?? string.Empty).Trim();
        if (id.Length == 0) return (null, "An inventory transaction is required.");

        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var (origin, transaction, error) = await LoadSideAsync(companyId, id, ct);
            if (error != null) return await Fail(error, ct);
            if (transaction!.MarkingRefInventTransOrigin == 0)
                return await Fail($"Inventory transaction '{id}' is not marked.", ct);
            if (transaction.ValueOpen == InventTransOpen.No)
                return await Fail("A financially settled transaction cannot be unmarked.", ct);

            // Marking is held on both sides, so removing it clears both.
            var counterpartOrigin = transaction.MarkingRefInventTransOrigin;
            var counterparts = await Db.Set<InventTrans>()
                .Where(x => x.DataAreaId == companyId && x.InventTransOrigin == counterpartOrigin
                    && x.MarkingRefInventTransOrigin == origin!.RecId)
                .ToListAsync(ct);
            foreach (var counterpart in counterparts) counterpart.MarkingRefInventTransOrigin = 0;
            transaction.MarkingRefInventTransOrigin = 0;
            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return (Map(transaction, id, string.Empty), null);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task<IReadOnlyList<InventoryMarkingRowDto>?> ListAsync(string? itemId, CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;
        itemId = itemId?.Trim();
        var marked = await Db.Set<InventTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.MarkingRefInventTransOrigin != 0 && x.Qty < 0
                && (itemId == null || itemId == "" || x.ItemId == itemId))
            .Select(x => new { x.RecId, x.ItemId, x.Qty, x.InventTransOrigin, x.MarkingRefInventTransOrigin, x.ValueOpen })
            .ToListAsync(ct);
        var originIds = marked.Select(x => x.InventTransOrigin)
            .Concat(marked.Select(x => x.MarkingRefInventTransOrigin)).Distinct().ToList();
        var origins = await Db.Set<InventTransOrigin>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && originIds.Contains(x.RecId))
            .Select(x => new { x.RecId, x.InventTransId })
            .ToDictionaryAsync(x => x.RecId, x => x.InventTransId, ct);
        return marked
            .Select(x => new InventoryMarkingRowDto(x.ItemId,
                origins.GetValueOrDefault(x.InventTransOrigin, string.Empty),
                origins.GetValueOrDefault(x.MarkingRefInventTransOrigin, string.Empty),
                x.Qty, x.ValueOpen == InventTransOpen.Yes))
            .OrderBy(x => x.ItemId, StringComparer.Ordinal)
            .ThenBy(x => x.IssueInventTransId, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// One side of a marking. Marking points at a whole transaction, so a side that has been split
    /// into several transactions is rejected instead of being marked in part.
    /// </summary>
    private async Task<(InventTransOrigin? Origin, InventTrans? Transaction, string? Error)> LoadSideAsync(
        string companyId, string inventTransId, CancellationToken ct)
    {
        var origin = await Db.Set<InventTransOrigin>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.InventTransId == inventTransId, ct);
        if (origin == null)
            return (null, null, $"Inventory transaction '{inventTransId}' was not found in the selected company.");
        var transactions = await Db.Set<InventTrans>()
            .Where(x => x.DataAreaId == companyId && x.InventTransOrigin == origin.RecId).ToListAsync(ct);
        return transactions.Count switch
        {
            0 => (null, null, $"Inventory transaction '{inventTransId}' has no inventory movement to mark."),
            1 => (origin, transactions[0], null),
            _ => (null, null,
                $"Inventory transaction '{inventTransId}' is split across {transactions.Count} movements. " +
                "Basic marking links whole transactions, so a split quantity cannot be marked.")
        };
    }

    private async Task<(InventoryMarkingDto?, string?)> Fail(string error, CancellationToken ct)
    {
        await unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return (null, error);
    }

    private static InventoryMarkingDto Map(InventTrans issue, string issueInventTransId, string receiptInventTransId) =>
        new(issue.ItemId, issueInventTransId, receiptInventTransId, issue.Qty,
            issue.MarkingRefInventTransOrigin != 0);
}
