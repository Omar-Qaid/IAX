using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using LedgerTable = IAX.IXApi.Modules.Finance.Entities.Ledger;

namespace IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;

/// <summary>
/// Defines, activates and assigns account structures against a DbContext, so the API service and
/// the seeder share one implementation instead of each building the tree its own way.
///
/// A structure is a DimensionHierarchy of type AccountStructure. Its segments are
/// DimensionHierarchyLevel rows in order, and its allowed values are a DimensionConstraintTree of
/// DimensionConstraintNode rows, one per segment, each carrying its
/// DimensionConstraintNodeCriteria. Posting validation reads that tree.
///
/// Defining writes the tree straight away and leaves the structure a draft, which is how D365
/// holds a draft — in the same tables, which is what their Temporary fields and the tree's
/// IsActivating flag are for. A draft's tree constrains nothing, because validation only consults
/// structures that are both activated and assigned to the ledger.
///
/// Scope is a structure with a single path: one set of allowed values per segment, which is what
/// the core cycle's balance sheet and profit and loss structures are. A structure whose grid holds
/// several rows, where a later segment's allowed values depend on the path taken through the
/// earlier ones, needs a node per branch; the tables model that, this does not build it.
/// </summary>
public sealed class AccountStructureBuilder(DbContext db)
{
    /// <summary>The main account is a segment of every structure, under its own dimension name.</summary>
    public const string MainAccountSegment = "MainAccount";

    /// <summary>One main account segment and up to ten financial dimensions.</summary>
    private const int MaximumSegments = 11;

    public async Task<(DimensionHierarchy? Structure, string? Error)> DefineAsync(
        string area, AccountStructureCreateDto input, CancellationToken ct)
    {
        var name = (input?.Name ?? string.Empty).Trim();
        if (name.Length == 0) return (null, "An account structure name is required.");
        if (input!.Segments == null || input.Segments.Count == 0)
            return (null, "An account structure needs at least one segment.");
        if (!input.Segments.Any(x => IsMainAccount(x.Dimension)))
            return (null, $"An account structure must include a '{MainAccountSegment}' segment.");
        if (input.Segments.Count > MaximumSegments)
            return (null, $"An account structure may hold at most {MaximumSegments} segments.");
        if (input.Segments.Select(x => (x.Dimension ?? string.Empty).Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase).Count() != input.Segments.Count)
            return (null, "A dimension may appear only once in an account structure.");
        if (await db.Set<DimensionHierarchy>().AnyAsync(x => x.DataAreaId == area && x.Name == name, ct))
            return (null, $"An account structure named '{name}' already exists.");

        // Resolve every segment before writing anything, so a bad definition leaves no partial structure.
        var resolved = new List<Segment>();
        foreach (var segment in input.Segments)
        {
            var dimension = (segment.Dimension ?? string.Empty).Trim();
            var attribute = 0L;
            if (!IsMainAccount(dimension))
            {
                var found = await db.Set<DimensionAttribute>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DataAreaId == area && x.Name == dimension, ct);
                if (found == null)
                    return (null, $"Financial dimension '{dimension}' does not exist in this company.");
                attribute = found.RecId;
            }
            var criteria = DimensionCriteriaMatcher.Parse(segment.AllowedValues);
            if (criteria.Count == 0) return (null, $"Segment '{dimension}' has no allowed values.");
            if (criteria.All(x => x.IsBlank))
                return (null, $"Segment '{dimension}' allows only blank, so it can never carry a value.");
            resolved.Add(new Segment(dimension, attribute, criteria));
        }

        var structure = new DimensionHierarchy
        {
            DataAreaId = area, Name = name, Description = (input.Description ?? string.Empty).Trim(),
            StructureType = DimensionHierarchyStructureType.AccountStructure,
            DraftName = name, IsDraft = NoYes.Yes, HashKey = Guid.NewGuid()
        };
        db.Add(structure);
        await db.SaveChangesAsync(ct);
        await WriteTreeAsync(area, structure, resolved, ct);
        return (structure, null);
    }

    public async Task<(DimensionHierarchy? Structure, string? Error)> ActivateAsync(
        string area, string name, CancellationToken ct)
    {
        var structure = await db.Set<DimensionHierarchy>()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.Name == (name ?? string.Empty).Trim(), ct);
        if (structure == null) return (null, null);

        var tree = await db.Set<DimensionConstraintTree>()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.DimensionHierarchy == structure.RecId, ct);
        if (tree == null) return (null, $"Account structure '{structure.Name}' has no segments to activate.");

        var mainAccountCriteria = await MainAccountCriteriaAsync(area, tree.RecId, ct);
        if (mainAccountCriteria.Count == 0)
            return (null, $"Account structure '{structure.Name}' has no main account values.");
        if (await OverlapsAnotherStructureAsync(area, structure.RecId, mainAccountCriteria, ct) is { } conflict)
            return (null, conflict);

        structure.IsDraft = NoYes.No;
        tree.LastModifiedId = Guid.NewGuid();
        tree.IsActivating = 0;
        await db.SaveChangesAsync(ct);
        return (structure, null);
    }

    public async Task<(DimensionHierarchy? Structure, string? Error)> AssignToLedgerAsync(
        string area, string name, CancellationToken ct)
    {
        var structure = await db.Set<DimensionHierarchy>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.Name == (name ?? string.Empty).Trim(), ct);
        if (structure == null) return (null, null);
        if (structure.IsDraft == NoYes.Yes)
            return (null, $"Activate '{structure.Name}' before assigning it to the ledger.");
        var ledger = await db.Set<LedgerTable>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == area, ct);
        if (ledger == null) return (null, "A ledger is required for the selected company.");

        if (!await db.Set<LedgerStructure>()
                .AnyAsync(x => x.Ledger == ledger.RecId && x.DimensionHierarchy == structure.RecId, ct))
        {
            db.Add(new LedgerStructure
            {
                DataAreaId = area, Ledger = ledger.RecId, DimensionHierarchy = structure.RecId
            });
            await db.SaveChangesAsync(ct);
        }
        return (structure, null);
    }

    public async Task<AccountStructureDto> DescribeAsync(string area, DimensionHierarchy structure, CancellationToken ct)
    {
        var levels = await db.Set<DimensionHierarchyLevel>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.DimensionHierarchy == structure.RecId)
            .OrderBy(x => x.Level).ToListAsync(ct);
        var names = await db.Set<DimensionAttribute>().AsNoTracking()
            .Where(x => x.DataAreaId == area).ToDictionaryAsync(x => x.RecId, x => x.Name, ct);
        var tree = await db.Set<DimensionConstraintTree>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.DimensionHierarchy == structure.RecId, ct);
        var nodes = tree == null
            ? new List<DimensionConstraintNode>()
            : await db.Set<DimensionConstraintNode>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.DimensionConstraintTree == tree.RecId).ToListAsync(ct);
        var nodeIds = nodes.Select(x => x.RecId).ToList();
        var criteria = nodeIds.Count == 0
            ? new List<DimensionConstraintNodeCriteria>()
            : await db.Set<DimensionConstraintNodeCriteria>().AsNoTracking()
                .Where(x => x.DataAreaId == area && nodeIds.Contains(x.DimensionConstraintNode))
                .OrderBy(x => x.Ordinal).ToListAsync(ct);
        var assigned = await db.Set<LedgerStructure>().AsNoTracking()
            .AnyAsync(x => x.DimensionHierarchy == structure.RecId, ct);

        var segments = levels.Select(level =>
        {
            var dimension = level.DimensionAttribute == 0
                ? MainAccountSegment
                : names.GetValueOrDefault(level.DimensionAttribute, string.Empty);
            var node = nodes.FirstOrDefault(x => x.DimensionHierarchyLevel == level.RecId);
            var allowsBlank = node?.IsOptional == NoYes.Yes;
            var parts = node == null
                ? new List<string>()
                : criteria.Where(x => x.DimensionConstraintNode == node.RecId)
                    .Select(x => x.WildCardString.Length > 0
                        ? x.WildCardString
                        : x.RangeFrom + DimensionCriteriaMatcher.RangeSeparator + x.RangeTo)
                    .ToList();
            if (allowsBlank) parts.Add("\"\"");
            return new AccountStructureSegmentResultDto(level.Level, dimension, string.Join(';', parts), allowsBlank);
        }).ToList();

        return new AccountStructureDto(structure.RecId, structure.Name, structure.Description,
            structure.IsDraft == NoYes.Yes, assigned, segments);
    }

    private sealed record Segment(string Dimension, long Attribute,
        IReadOnlyList<DimensionCriteriaMatcher.Criterion> Criteria);

    private async Task WriteTreeAsync(string area, DimensionHierarchy structure,
        List<Segment> segments, CancellationToken ct)
    {
        var tree = new DimensionConstraintTree
        {
            DataAreaId = area, DimensionHierarchy = structure.RecId, LastModifiedId = Guid.NewGuid()
        };
        db.Add(tree);
        await db.SaveChangesAsync(ct);

        var parent = 0L;
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            var level = new DimensionHierarchyLevel
            {
                DataAreaId = area, DimensionHierarchy = structure.RecId,
                DimensionAttribute = segment.Attribute, Level = i + 1
            };
            db.Add(level);
            await db.SaveChangesAsync(ct);

            var node = new DimensionConstraintNode
            {
                DataAreaId = area, DimensionConstraintTree = tree.RecId,
                DimensionHierarchyLevel = level.RecId, ParentConstraintNode = parent,
                Ordinal = 1, Name = segment.Dimension,
                // Blank is allowed only when the criteria say so, with a "" of their own: `*`
                // allows every non-blank value and nothing more.
                IsOptional = segment.Criteria.Any(x => x.IsBlank) ? NoYes.Yes : NoYes.No
            };
            db.Add(node);
            await db.SaveChangesAsync(ct);

            var ordinal = 0;
            foreach (var criterion in segment.Criteria.Where(x => !x.IsBlank))
                db.Add(new DimensionConstraintNodeCriteria
                {
                    DataAreaId = area, DimensionConstraintNode = node.RecId, Ordinal = ++ordinal,
                    RangeFrom = criterion.RangeFrom, RangeTo = criterion.RangeTo,
                    WildCardString = criterion.WildCardString
                });
            parent = node.RecId;
        }
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// The main account value selects the structure, so two active structures on the ledger may not
    /// both accept one. Only activated structures are compared, as D365 compares the draft being
    /// activated against the active versions of the others.
    /// </summary>
    private async Task<string?> OverlapsAnotherStructureAsync(string area, long structureRecId,
        IReadOnlyList<DimensionCriteriaMatcher.Criterion> mainAccountCriteria, CancellationToken ct)
    {
        var others = await (from tree in db.Set<DimensionConstraintTree>().AsNoTracking()
                            join structure in db.Set<DimensionHierarchy>().AsNoTracking()
                                on tree.DimensionHierarchy equals structure.RecId
                            where tree.DataAreaId == area && structure.IsDraft == NoYes.No
                                && structure.RecId != structureRecId
                            select new { Tree = tree.RecId, structure.Name }).ToListAsync(ct);
        foreach (var other in others)
        {
            var criteria = await MainAccountCriteriaAsync(area, other.Tree, ct);
            if (criteria.Count > 0 && DimensionCriteriaMatcher.MayOverlap(mainAccountCriteria, criteria))
                return $"Main account values overlap active account structure '{other.Name}'. "
                    + "A main account value may belong to only one structure on the ledger.";
        }
        return null;
    }

    private async Task<IReadOnlyList<DimensionCriteriaMatcher.Criterion>> MainAccountCriteriaAsync(
        string area, long treeRecId, CancellationToken ct)
    {
        var node = await db.Set<DimensionConstraintNode>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.DimensionConstraintTree == treeRecId
                && x.Name == MainAccountSegment, ct);
        if (node == null) return [];
        var rows = await db.Set<DimensionConstraintNodeCriteria>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.DimensionConstraintNode == node.RecId)
            .OrderBy(x => x.Ordinal).ToListAsync(ct);
        return rows.Select(x => new DimensionCriteriaMatcher.Criterion(
            x.RangeFrom, x.RangeTo, x.WildCardString, IsBlank: false)).ToList();
    }

    internal static bool IsMainAccount(string? dimension) =>
        string.Equals((dimension ?? string.Empty).Trim(), MainAccountSegment, StringComparison.OrdinalIgnoreCase);
}
