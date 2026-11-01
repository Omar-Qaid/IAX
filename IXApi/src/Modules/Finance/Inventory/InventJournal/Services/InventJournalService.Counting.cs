using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

/// <summary>
/// Creating the lines of a counting journal from on-hand inventory, the way a stock count actually
/// starts: the counter selects a scope - an item, an item group, a count group or a warehouse - and
/// the journal is filled with one line per item and inventory dimension in that scope.
///
/// A candidate must pass exactly the same line validation as a hand-entered line, so generation
/// cannot introduce a line that posting would later refuse. Candidates that fail are skipped and
/// reported, because one unready item must not stop the whole count.
/// </summary>
public sealed partial class InventJournalService
{
    /// <summary>The number of candidate lines one call will create, so a mistaken scope cannot flood a journal.</summary>
    private const int MaxGeneratedCountingLines = 1000;

    public async Task<(InventCountingLinesResultDto? Result, string? Error)> CreateCountingLinesAsync(
        string journalId, InventCountingLinesInputDto input, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var header = await EditableHeaderAsync(companyId, journalId, ct);
        if (header == null) return (null, "The journal was not found or has already been posted.");
        if (header.JournalType != InventJournalType.Counting)
            return (null, "Lines can only be generated for a counting journal.");
        if (header.CountingFrozen)
            return (null, "The count is frozen. Cancel the count before changing its lines.");

        var itemId = Normalize(input.ItemId);
        var itemGroupId = Normalize(input.ItemGroupId);
        var countGroupId = Normalize(input.CountGroupId);
        var siteId = Normalize(input.InventSiteId);
        var warehouseId = Normalize(input.InventLocationId);
        var transDate = (input.TransDate ?? DateTime.UtcNow).Date;

        // The journal's own site and warehouse narrow the scope when the request does not.
        if (siteId.Length == 0) siteId = header.InventSiteId;
        if (warehouseId.Length == 0) warehouseId = header.InventLocationId;

        var candidates = await (
            from summary in Db.Set<InventSum>().AsNoTracking()
            join dimension in Db.Set<InventDim>().AsNoTracking()
                on new { summary.DataAreaId, summary.InventDimId }
                equals new { dimension.DataAreaId, dimension.InventDimId }
            where summary.DataAreaId == companyId
                && (itemId.Length == 0 || summary.ItemId == itemId)
                && (siteId.Length == 0 || dimension.InventSiteId == siteId)
                && (warehouseId.Length == 0 || dimension.InventLocationId == warehouseId)
                && (!input.OnHandOnly || summary.PhysicalInvent != 0)
                && (itemGroupId.Length == 0 || Db.Set<InventItemGroupItem>().Any(assignment =>
                        assignment.ItemDataAreaId == companyId && assignment.ItemGroupDataAreaId == companyId
                        && assignment.ItemId == summary.ItemId && assignment.ItemGroupId == itemGroupId))
                && (countGroupId.Length == 0 || Db.Set<InventItemLocation>().Any(setup =>
                        setup.DataAreaId == companyId && setup.ItemId == summary.ItemId
                        && setup.InventDimId == summary.InventDimId && setup.CountGroupId == countGroupId))
            orderby summary.ItemId, dimension.InventSiteId, dimension.InventLocationId
            select new
            {
                summary.ItemId,
                summary.InventDimId,
                dimension.InventSiteId,
                dimension.InventLocationId,
                dimension.InventBatchId,
                dimension.InventSerialId,
                dimension.InventStatusId,
                dimension.WmsLocationId
            }).ToListAsync(ct);

        var existing = (await Db.Set<InventJournalTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.JournalId == header.JournalId)
            .Select(x => new { x.ItemId, x.InventDimId }).ToListAsync(ct))
            .Select(x => (x.ItemId, x.InventDimId)).ToHashSet();
        var lineNumbers = await Db.Set<InventJournalTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.JournalId == header.JournalId)
            .Select(x => x.LineNum).ToListAsync(ct);
        var lineNum = lineNumbers.Count == 0 ? 0m : lineNumbers.Max();

        var units = await Db.Set<InventTableModule>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.ModuleType == ModuleInventPurchSales.Inventory)
            .Select(x => new { x.ItemId, x.UnitId }).ToDictionaryAsync(x => x.ItemId, x => x.UnitId, ct);

        var created = 0;
        var skipped = 0;
        var problems = new List<string>();
        foreach (var candidate in candidates)
        {
            if (created >= MaxGeneratedCountingLines)
            {
                problems.Add($"More than {MaxGeneratedCountingLines} lines match this scope. Narrow the scope and generate the rest in a second run.");
                break;
            }
            if (existing.Contains((candidate.ItemId, candidate.InventDimId))) { skipped++; continue; }

            // The basic counting journal counts a site and warehouse dimension only.
            if (candidate.InventBatchId.Length > 0 || candidate.InventSerialId.Length > 0 ||
                candidate.InventStatusId.Length > 0 || candidate.WmsLocationId.Length > 0)
            {
                skipped++;
                problems.Add($"Item '{candidate.ItemId}' holds stock on a batch, serial, status or location dimension, which the basic counting journal does not count.");
                continue;
            }
            if (!units.TryGetValue(candidate.ItemId, out var unit) || string.IsNullOrWhiteSpace(unit))
            {
                skipped++;
                problems.Add($"Item '{candidate.ItemId}' has no inventory unit.");
                continue;
            }

            var line = new InventJournalLineInputDto
            {
                TransDate = transDate,
                ItemId = candidate.ItemId,
                InventSiteId = candidate.InventSiteId,
                InventLocationId = candidate.InventLocationId,
                Unit = unit,
                Counted = 0m
            };
            var validation = await ValidateLineAsync(companyId, header, line, ct);
            if (validation.Error != null)
            {
                skipped++;
                problems.Add($"Item '{candidate.ItemId}' at {candidate.InventSiteId}/{candidate.InventLocationId}: {validation.Error}");
                continue;
            }
            var entity = new InventJournalTrans
            {
                DataAreaId = companyId, JournalId = header.JournalId, LineNum = ++lineNum
            };
            ApplyLine(entity, header, line, validation.FromDimension!, validation.ToDimension);
            Db.Add(entity);
            existing.Add((candidate.ItemId, candidate.InventDimId));
            created++;
        }

        if (created > 0)
        {
            header.NumOfLines += created;
            await _unitOfWork.CompleteAsync(ct);
        }
        return (new InventCountingLinesResultDto(created, skipped, problems,
            await MapWithLinesAsync(header, companyId, ct)), null);
    }
}
