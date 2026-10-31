using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;
using Microsoft.EntityFrameworkCore;
using LedgerTable = IAX.IXApi.Modules.Finance.Entities.Ledger;

namespace IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;

/// <summary>
/// Validates a ledger account combination against the account structures assigned to the ledger.
///
/// The main account value selects the structure — activation guarantees only one active structure
/// claims it — and each remaining segment's value must satisfy that structure's criteria for its
/// level. A blank segment is allowed only where the structure marks the segment optional, which is
/// the rule behind the core cycle's acceptance example: a blank Department is valid on a balance
/// sheet account and invalid on a profit and loss account.
///
/// **A company with no assigned structure is not validated**, and posting proceeds as it did before
/// this existed. D365 requires a structure on the ledger; this does not, because every ledger
/// account combination already in this database is main-account-only, and refusing them all would
/// stop every company and test fixture from posting. The journal setup check reports the gap so it
/// is visible rather than silent.
///
/// The structures are read once per instance. One instance is held per posting operation, so a
/// journal with many lines loads the setup once rather than per line.
/// </summary>
public sealed class AccountStructureValidator(DbContext db)
{
    private Snapshot? _snapshot;

    /// <summary>The refusal for this combination, or null when it is allowed.</summary>
    public async Task<string?> ValidateAsync(string area, DimensionAttributeValueCombination combination,
        CancellationToken ct)
    {
        var snapshot = _snapshot ??= await LoadAsync(area, ct);
        if (snapshot.Structures.Count == 0) return null; // Nothing assigned: not validated.

        var accountValue = combination.MainAccountValue.Length > 0
            ? combination.MainAccountValue
            : snapshot.MainAccountIds.GetValueOrDefault(combination.MainAccount, string.Empty);
        if (accountValue.Length == 0)
            return $"Ledger account '{combination.DisplayValue}' has no main account value to validate.";

        var matched = snapshot.Structures
            .Where(x => DimensionCriteriaMatcher.Allows(x.MainAccount.Criteria, NoYes.No, accountValue))
            .ToList();
        if (matched.Count == 0)
            return $"Main account '{accountValue}' is not covered by any account structure assigned to the ledger.";
        if (matched.Count > 1)
            return $"Main account '{accountValue}' is claimed by account structures "
                + $"{string.Join(" and ", matched.Select(x => $"'{x.Name}'"))}; it may belong to only one.";

        var structure = matched[0];
        var values = await SegmentValuesAsync(area, combination, ct);
        foreach (var segment in structure.Segments)
        {
            var value = values.GetValueOrDefault(segment.Dimension, string.Empty);
            if (DimensionCriteriaMatcher.Allows(segment.Criteria, segment.IsOptional, value)) continue;
            return value.Length == 0
                ? $"Account structure '{structure.Name}' requires a {segment.Dimension} on main account '{accountValue}'."
                : $"'{value}' is not an allowed {segment.Dimension} for main account '{accountValue}' "
                    + $"under account structure '{structure.Name}'.";
        }
        return null;
    }

    /// <summary>
    /// The combination's value per dimension. The value group chain is authoritative; the
    /// combination's own per-dimension column is the fallback, so a combination written without a
    /// chain is still validated rather than silently read as all blanks.
    /// </summary>
    private async Task<Dictionary<string, string>> SegmentValuesAsync(string area,
        DimensionAttributeValueCombination combination, CancellationToken ct)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (dimension, value) in await new LedgerDimensionResolver(db)
                     .SegmentValuesAsync(area, combination.RecId, ct))
            values[dimension] = value;
        foreach (var dimension in (_snapshot?.Dimensions ?? []).Where(x => !values.ContainsKey(x)))
        {
            var column = LedgerDimensionResolver.DenormalisedValue(combination, dimension);
            if (!string.IsNullOrEmpty(column)) values[dimension] = column;
        }
        return values;
    }

    private async Task<Snapshot> LoadAsync(string area, CancellationToken ct)
    {
        var ledger = await db.Set<LedgerTable>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area, ct);
        if (ledger == null) return Snapshot.Empty;

        var assigned = await (from link in db.Set<LedgerStructure>().AsNoTracking()
                              join structure in db.Set<DimensionHierarchy>().AsNoTracking()
                                  on link.DimensionHierarchy equals structure.RecId
                              where link.Ledger == ledger.RecId && structure.IsDraft == NoYes.No
                              select structure).ToListAsync(ct);
        if (assigned.Count == 0) return Snapshot.Empty;

        var structureIds = assigned.Select(x => x.RecId).ToList();
        var trees = await db.Set<DimensionConstraintTree>().AsNoTracking()
            .Where(x => x.DataAreaId == area && structureIds.Contains(x.DimensionHierarchy)).ToListAsync(ct);
        var treeIds = trees.Select(x => x.RecId).ToList();
        var nodes = await db.Set<DimensionConstraintNode>().AsNoTracking()
            .Where(x => x.DataAreaId == area && treeIds.Contains(x.DimensionConstraintTree)).ToListAsync(ct);
        var nodeIds = nodes.Select(x => x.RecId).ToList();
        var criteria = await db.Set<DimensionConstraintNodeCriteria>().AsNoTracking()
            .Where(x => x.DataAreaId == area && nodeIds.Contains(x.DimensionConstraintNode))
            .OrderBy(x => x.Ordinal).ToListAsync(ct);
        var levels = await db.Set<DimensionHierarchyLevel>().AsNoTracking()
            .Where(x => x.DataAreaId == area && structureIds.Contains(x.DimensionHierarchy)).ToListAsync(ct);
        var attributeNames = await db.Set<DimensionAttribute>().AsNoTracking()
            .Where(x => x.DataAreaId == area).ToDictionaryAsync(x => x.RecId, x => x.Name, ct);
        var mainAccountIds = await db.Set<MainAccount>().AsNoTracking()
            .Where(x => x.DataAreaId == area).ToDictionaryAsync(x => x.RecId, x => x.MainAccountId, ct);

        var structures = new List<StructureRules>();
        foreach (var structure in assigned)
        {
            var tree = trees.FirstOrDefault(x => x.DimensionHierarchy == structure.RecId);
            if (tree == null) continue; // Never activated into a tree, so it constrains nothing.
            var segments = new List<SegmentRules>();
            SegmentRules? mainAccount = null;
            foreach (var level in levels.Where(x => x.DimensionHierarchy == structure.RecId).OrderBy(x => x.Level))
            {
                var node = nodes.FirstOrDefault(x => x.DimensionConstraintTree == tree.RecId
                    && x.DimensionHierarchyLevel == level.RecId);
                if (node == null) continue;
                var rules = new SegmentRules(
                    level.DimensionAttribute == 0
                        ? AccountStructureBuilder.MainAccountSegment
                        : attributeNames.GetValueOrDefault(level.DimensionAttribute, string.Empty),
                    criteria.Where(x => x.DimensionConstraintNode == node.RecId).ToList(),
                    node.IsOptional);
                if (level.DimensionAttribute == 0) mainAccount = rules;
                else segments.Add(rules);
            }
            if (mainAccount == null) continue; // No main account segment, so nothing selects it.
            structures.Add(new StructureRules(structure.Name, mainAccount, segments));
        }

        return new Snapshot(structures, mainAccountIds,
            structures.SelectMany(x => x.Segments).Select(x => x.Dimension)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList());
    }

    private sealed record SegmentRules(string Dimension,
        IReadOnlyList<DimensionConstraintNodeCriteria> Criteria, NoYes IsOptional);

    private sealed record StructureRules(string Name, SegmentRules MainAccount, IReadOnlyList<SegmentRules> Segments);

    private sealed record Snapshot(IReadOnlyList<StructureRules> Structures,
        IReadOnlyDictionary<long, string> MainAccountIds, IReadOnlyList<string> Dimensions)
    {
        public static Snapshot Empty { get; } = new([], new Dictionary<long, string>(), []);
    }
}
