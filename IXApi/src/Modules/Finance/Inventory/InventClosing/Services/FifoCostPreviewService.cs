using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using ItemModelGroup = IAX.IXApi.Modules.Finance.Entities.InventModelGroup;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record FifoCostAllocationDto(long ReceiptRecId, decimal Quantity, decimal UnitCost, decimal Cost);
public sealed record FifoCostIssueDto(long IssueRecId, string ItemId, string SiteId, DateTime FinancialDate,
    decimal Quantity, decimal RecognizedCost, decimal FifoCost, decimal RemainingAdjustment,
    IReadOnlyList<FifoCostAllocationDto> Allocations);
public sealed record FifoCostPreviewDto(DateTime Cutoff, bool CanExecute, IReadOnlyList<string> Problems,
    IReadOnlyList<FifoCostIssueDto> Issues);

public interface IFifoCostPreviewService
{
    Task<FifoCostPreviewDto> PreviewAsync(DateTime cutoff, CancellationToken ct = default);
}

public sealed class FifoCostPreviewService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
    : IFifoCostPreviewService
{
    public async Task<FifoCostPreviewDto> PreviewAsync(DateTime cutoff, CancellationToken ct = default)
    {
        var area = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area))
            return new FifoCostPreviewDto(cutoff.Date, false, ["A company must be selected."], []);

        var problems = new List<string>();
        var db = unitOfWork.Context;
        var end = cutoff.Date.AddDays(1);
        if (await db.Set<InventSettlement>().AsNoTracking().AnyAsync(x => x.DataAreaId == area, ct) ||
            await db.Set<InventClosing>().AsNoTracking().AnyAsync(x => x.DataAreaId == area, ct))
            problems.Add("Existing settlement or closing history is present; this simple preview does not reconstruct prior open quantities.");
        var transactions = await db.Set<InventTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.DateFinancial < end &&
                (x.StatusReceipt == StatusReceipt.Purchased || x.StatusIssue == StatusIssue.Sold))
            .OrderBy(x => x.DateFinancial).ThenBy(x => x.RecId).ToListAsync(ct);
        var dimensionIds = transactions.Select(x => x.InventDimId).Distinct().ToList();
        var dimensions = await db.Set<InventDim>().AsNoTracking()
            .Where(x => x.DataAreaId == area && dimensionIds.Contains(x.InventDimId))
            .ToDictionaryAsync(x => x.InventDimId, ct);
        var originIds = transactions.Select(x => x.InventTransOrigin).Distinct().ToList();
        var origins = await db.Set<InventTransOrigin>().AsNoTracking()
            .Where(x => x.DataAreaId == area && originIds.Contains(x.RecId))
            .ToDictionaryAsync(x => x.RecId, ct);
        var transferPairs = await db.Set<InventJournalTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.JournalType == InventJournalType.Transfer)
            .Select(x => new { x.InventTransId, x.ToInventTransId }).ToListAsync(ct);
        var transferSet = transferPairs.SelectMany(x => new[] { x.InventTransId, x.ToInventTransId })
            .ToHashSet(StringComparer.Ordinal);
        var assignments = await db.Set<InventModelGroupItem>().AsNoTracking()
            .Where(x => x.ItemDataAreaId == area).ToListAsync(ct);
        var models = await db.Set<ItemModelGroup>().AsNoTracking()
            .Where(x => x.DataAreaId == area).ToDictionaryAsync(x => x.ModelGroupId, ct);

        var rows = new List<(InventTrans Transaction, string Site)>();
        foreach (var transaction in transactions)
        {
            if (!dimensions.TryGetValue(transaction.InventDimId, out var dim) || string.IsNullOrWhiteSpace(dim.InventSiteId))
            {
                problems.Add($"Transaction {transaction.RecId} has no valid financial site dimension.");
                continue;
            }
            if (!origins.TryGetValue(transaction.InventTransOrigin, out var origin) ||
                origin.ReferenceCategory != InventRefType.InventoryJournal || transferSet.Contains(origin.InventTransId))
            {
                problems.Add($"Transaction {transaction.RecId} needs a verified source or transfer cost relationship.");
                continue;
            }
            var assignment = assignments.FirstOrDefault(x => x.ItemId == transaction.ItemId);
            if (assignment is null || !models.TryGetValue(assignment.ModelGroupId, out var model) ||
                model.InventModel != InventModel.FIFO)
            {
                problems.Add($"Item {transaction.ItemId} does not have a verified FIFO model group.");
                continue;
            }
            rows.Add((transaction, dim.InventSiteId));
        }

        var issues = new List<FifoCostIssueDto>();
        foreach (var pool in rows.GroupBy(x => (x.Transaction.ItemId, x.Site)))
        {
            var layers = new LinkedList<(long RecId, decimal Remaining, decimal UnitCost)>();
            foreach (var row in pool.OrderBy(x => x.Transaction.DateFinancial).ThenBy(x => x.Transaction.RecId))
            {
                var trans = row.Transaction;
                if (trans.Qty > 0 && trans.StatusReceipt == StatusReceipt.Purchased)
                {
                    var value = trans.CostAmountPosted + trans.CostAmountAdjustment;
                    if (value <= 0)
                        problems.Add($"Receipt {trans.RecId} has no positive financial cost.");
                    else
                        layers.AddLast((trans.RecId, trans.Qty, value / trans.Qty));
                    continue;
                }
                if (trans.Qty >= 0 || trans.StatusIssue != StatusIssue.Sold)
                {
                    problems.Add($"Transaction {trans.RecId} has an unsupported financial quantity/status combination.");
                    continue;
                }
                var needed = -trans.Qty;
                var allocations = new List<FifoCostAllocationDto>();
                while (needed > 0 && layers.Count > 0)
                {
                    var layer = layers.First!.Value;
                    var quantity = Math.Min(needed, layer.Remaining);
                    allocations.Add(new FifoCostAllocationDto(layer.RecId, quantity, layer.UnitCost, quantity * layer.UnitCost));
                    needed -= quantity;
                    if (layer.Remaining > quantity)
                        layers.First.Value = (layer.RecId, layer.Remaining - quantity, layer.UnitCost);
                    else
                        layers.RemoveFirst();
                }
                if (needed > 0)
                {
                    problems.Add($"Issue {trans.RecId} lacks {needed} financially received units in its FIFO pool.");
                    continue;
                }
                var target = -allocations.Sum(x => x.Cost);
                var recognized = trans.CostAmountPosted + trans.CostAmountAdjustment;
                issues.Add(new FifoCostIssueDto(trans.RecId, pool.Key.ItemId, pool.Key.Site,
                    trans.DateFinancial, -trans.Qty, recognized, target, target - recognized, allocations));
            }
        }
        problems.Add("Close execution is unavailable until settlement, original posting accounts, and reversal rules are verified.");
        return new FifoCostPreviewDto(cutoff.Date, false, problems.Distinct().ToList(), issues);
    }
}
