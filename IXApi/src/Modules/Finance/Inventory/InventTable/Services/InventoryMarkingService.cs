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
///
/// Where a side has already been split into several movements, the caller names the movement to
/// mark by its <see cref="InventTrans.RecId"/>; marking stays per transaction, as in D365.
/// </summary>
public sealed class InventoryMarkingService(
    IUnitOfWork unitOfWork, ICompanyExecutionContext company) : IInventoryMarkingService
{
    private DbContext Db => unitOfWork.Context;

    public Task<(InventoryMarkingDto? Result, string? Error)> MarkAsync(string issueInventTransId,
        string receiptInventTransId, CancellationToken ct = default) =>
        MarkAsync(issueInventTransId, receiptInventTransId, null, null, ct);

    public Task<(InventoryMarkingDto? Result, string? Error)> MarkAsync(string issueInventTransId,
        string receiptInventTransId, long? issueTransRecId, long? receiptTransRecId,
        CancellationToken ct = default) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() =>
            ApplyAsync(issueInventTransId, receiptInventTransId, issueTransRecId, receiptTransRecId, ct));

    public Task<(InventoryMarkingDto? Result, string? Error)> UnmarkAsync(string inventTransId,
        CancellationToken ct = default) =>
        UnmarkAsync(inventTransId, null, ct);

    public Task<(InventoryMarkingDto? Result, string? Error)> UnmarkAsync(string inventTransId,
        long? inventTransRecId, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => ClearAsync(inventTransId, inventTransRecId, ct));

    private async Task<(InventoryMarkingDto? Result, string? Error)> ApplyAsync(string issueInventTransId,
        string receiptInventTransId, long? issueTransRecId, long? receiptTransRecId, CancellationToken ct)
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
            await new SubledgerPostingSupport(Db).LockAsync($"InventMarking:{companyId}", ct);
            var (issueOrigin, issue, issueError) = await LoadSideAsync(companyId, issueId, issueTransRecId, ct);
            if (issueError != null) return await Fail(issueError, ct);
            var (receiptOrigin, receipt, receiptError) = await LoadSideAsync(companyId, receiptId, receiptTransRecId, ct);
            if (receiptError != null) return await Fail(receiptError, ct);

            if (issue!.Qty >= 0) return await Fail($"Inventory transaction '{issueId}' is not an issue.", ct);
            if (receipt!.Qty <= 0) return await Fail($"Inventory transaction '{receiptId}' is not a receipt.", ct);
            if (!string.Equals(issue.ItemId, receipt.ItemId, StringComparison.Ordinal))
                return await Fail("An issue can only be marked to a receipt of the same item.", ct);
            var dimensionIds = new[] { issue.InventDimId, receipt.InventDimId }.Distinct().ToArray();
            var dimensions = await Db.Set<InventDim>().AsNoTracking()
                .Where(x => x.DataAreaId == companyId && dimensionIds.Contains(x.InventDimId))
                .Select(x => new { x.InventDimId, x.InventSiteId }).ToListAsync(ct);
            var issueSite = dimensions.SingleOrDefault(x => x.InventDimId == issue.InventDimId)?.InventSiteId;
            var receiptSite = dimensions.SingleOrDefault(x => x.InventDimId == receipt.InventDimId)?.InventSiteId;
            if (string.IsNullOrWhiteSpace(issueSite) || string.IsNullOrWhiteSpace(receiptSite))
                return await Fail("Both marked transactions require a valid inventory site.", ct);
            if (!string.Equals(issueSite, receiptSite, StringComparison.Ordinal))
                return await Fail("An issue can only be marked to a receipt in the same financial site.", ct);
            if (issue.ValueOpen == InventTransOpen.No || receipt.ValueOpen == InventTransOpen.No)
                return await Fail("A financially settled transaction cannot be marked.", ct);

            // The references identify origins, not movements. Two pairs between the same
            // origins cannot be distinguished reliably when either pair is later removed.
            var ambiguousPair = await Db.Set<InventTrans>().AnyAsync(x => x.DataAreaId == companyId
                && ((x.InventTransOrigin == issueOrigin!.RecId && x.RecId != issue.RecId
                        && x.MarkingRefInventTransOrigin == receiptOrigin!.RecId)
                    || (x.InventTransOrigin == receiptOrigin!.RecId && x.RecId != receipt.RecId
                        && x.MarkingRefInventTransOrigin == issueOrigin!.RecId)), ct);
            if (ambiguousPair)
                return await Fail("Another marked movement already links these inventory origins; the individual pairs cannot be identified safely.", ct);

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
        catch (SubledgerPostingException ex)
        {
            return await Fail(ex.Message, ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    private async Task<(InventoryMarkingDto? Result, string? Error)> ClearAsync(string inventTransId, long? inventTransRecId, CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        var id = (inventTransId ?? string.Empty).Trim();
        if (id.Length == 0) return (null, "An inventory transaction is required.");

        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await new SubledgerPostingSupport(Db).LockAsync($"InventMarking:{companyId}", ct);
            var (origin, transaction, error) = await LoadSideAsync(companyId, id, inventTransRecId, ct);
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
            if (counterparts.Count != 1)
                return await Fail("The marked counterpart movement cannot be uniquely identified; the marking was not removed.", ct);
            if (counterparts[0].ValueOpen == InventTransOpen.No)
                return await Fail("A financially settled transaction cannot be unmarked.", ct);
            counterparts[0].MarkingRefInventTransOrigin = 0;
            transaction.MarkingRefInventTransOrigin = 0;
            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return (Map(transaction, id, string.Empty), null);
        }
        catch (SubledgerPostingException ex)
        {
            return await Fail(ex.Message, ct);
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
    /// The origin and the single movement to mark. A side split across several movements requires
    /// the caller to name one of them, because the marking reference is one field per movement.
    /// </summary>
    private async Task<(InventTransOrigin? Origin, InventTrans? Transaction, string? Error)> LoadSideAsync(
        string companyId, string inventTransId, long? transRecId, CancellationToken ct)
    {
        var origin = await Db.Set<InventTransOrigin>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.InventTransId == inventTransId, ct);
        if (origin == null)
            return (null, null, $"Inventory transaction '{inventTransId}' was not found in the selected company.");
        var transactions = await Db.Set<InventTrans>()
            .Where(x => x.DataAreaId == companyId && x.InventTransOrigin == origin.RecId).ToListAsync(ct);
        if (transactions.Count == 0)
            return (null, null, $"Inventory transaction '{inventTransId}' has no inventory movement to mark.");
        if (transRecId is long selected)
        {
            var movement = transactions.SingleOrDefault(x => x.RecId == selected);
            return movement is null
                ? (null, null, $"Movement {selected} does not belong to inventory transaction '{inventTransId}'.")
                : (origin, movement, null);
        }
        return transactions.Count == 1
            ? (origin, transactions[0], null)
            : (null, null, $"Inventory transaction '{inventTransId}' is split across {transactions.Count} movements. " +
                "Name the movement to mark, because marking links one movement at a time.");
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
