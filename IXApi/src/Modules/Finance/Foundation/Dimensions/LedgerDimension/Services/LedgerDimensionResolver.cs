using System.Reflection;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;

/// <summary>Raised when a ledger account combination cannot be resolved; mapped to a 422.</summary>
public sealed class LedgerDimensionException(string message) : Exception(message);

/// <summary>
/// Finds or creates the <see cref="DimensionAttributeValueCombination"/> for a main account and a
/// set of dimension values, with the value group chain behind it.
///
/// Nothing outside the seeders created a combination before this, and every seeded one is
/// main-account-only: display value equal to the account code, AccountStructure 0, no segment
/// values. A combination that carries no dimension values is why account structure validation had
/// nothing to validate.
///
/// Two representations are written, as D365 keeps both:
/// the normalised chain — DimensionAttributeValueGroup, one DimensionAttributeLevelValue per
/// segment, and the DimensionAttributeValueGroupCombination link — which is authoritative and works
/// for any dimension, and the denormalised per-dimension columns on the combination itself
/// (Department/DepartmentValue and friends), filled where a column of that name exists.
/// </summary>
public sealed class LedgerDimensionResolver(DbContext db)
{
    /// <summary>The display value separator D365 uses between segments of a ledger account.</summary>
    private const char SegmentSeparator = '-';

    /// <summary>
    /// The combination for this main account and these dimension values, created if it does not
    /// exist. <paramref name="values"/> is ordered by the structure's levels, and a blank value is
    /// a segment deliberately left empty — it is kept, not dropped, so the display value keeps its
    /// shape and the structure can decide whether a blank is allowed.
    /// </summary>
    public async Task<DimensionAttributeValueCombination> ResolveAsync(string area, MainAccount account,
        IReadOnlyList<(string Dimension, string Value)> values, long accountStructure, CancellationToken ct)
    {
        var displayValue = BuildDisplayValue(account.MainAccountId, values);
        if (displayValue.Length > FieldLengths.DisplayValue)
            throw new LedgerDimensionException(
                $"Ledger account '{displayValue}' exceeds {FieldLengths.DisplayValue} characters.");

        var existing = await db.Set<DimensionAttributeValueCombination>()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.DisplayValue == displayValue, ct);
        if (existing != null) return existing;

        var combination = new DimensionAttributeValueCombination
        {
            DataAreaId = area, DisplayValue = displayValue, AccountStructure = accountStructure,
            MainAccount = account.RecId, MainAccountValue = account.MainAccountId
        };

        var resolved = new List<(DimensionAttributeValue Value, string Dimension)>();
        foreach (var (dimension, value) in values)
        {
            if (string.IsNullOrEmpty(value)) continue; // A blank segment has no dimension value to link.
            resolved.Add((await RequireValueAsync(area, dimension, value, ct), dimension));
            SetDenormalisedColumn(combination, dimension, value, resolved[^1].Value.RecId);
        }

        db.Add(combination);
        await db.SaveChangesAsync(ct); // The group link needs the combination's key.

        var group = new DimensionAttributeValueGroup
        {
            DataAreaId = area, DimensionHierarchy = accountStructure, Levels = resolved.Count
        };
        db.Add(group);
        await db.SaveChangesAsync(ct);

        for (var i = 0; i < resolved.Count; i++)
            db.Add(new DimensionAttributeLevelValue
            {
                DataAreaId = area, DimensionAttributeValueGroup = group.RecId,
                DimensionAttributeValue = resolved[i].Value.RecId, Ordinal = i + 1,
                DisplayValue = resolved[i].Value.DisplayValue
            });
        db.Add(new DimensionAttributeValueGroupCombination
        {
            DataAreaId = area, DimensionAttributeValueCombination = combination.RecId,
            DimensionAttributeValueGroup = group.RecId, Ordinal = 1
        });
        await db.SaveChangesAsync(ct);
        return combination;
    }

    /// <summary>The segment values behind a combination, in level order, for structure validation.</summary>
    public async Task<IReadOnlyList<(string Dimension, string Value)>> SegmentValuesAsync(
        string area, long combinationRecId, CancellationToken ct)
    {
        var query = from link in db.Set<DimensionAttributeValueGroupCombination>().AsNoTracking()
                    join level in db.Set<DimensionAttributeLevelValue>().AsNoTracking()
                        on link.DimensionAttributeValueGroup equals level.DimensionAttributeValueGroup
                    join value in db.Set<DimensionAttributeValue>().AsNoTracking()
                        on level.DimensionAttributeValue equals value.RecId
                    join attribute in db.Set<DimensionAttribute>().AsNoTracking()
                        on value.DimensionAttribute equals attribute.RecId
                    where link.DataAreaId == area && link.DimensionAttributeValueCombination == combinationRecId
                    orderby link.Ordinal, level.Ordinal
                    select new { attribute.Name, level.DisplayValue };
        return (await query.ToListAsync(ct)).Select(x => (x.Name, x.DisplayValue)).ToList();
    }

    private async Task<DimensionAttributeValue> RequireValueAsync(string area, string dimension, string value, CancellationToken ct)
    {
        var attribute = await db.Set<DimensionAttribute>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.Name == dimension, ct)
            ?? throw new LedgerDimensionException($"Financial dimension '{dimension}' does not exist in this company.");
        var resolved = await db.Set<DimensionAttributeValue>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.DimensionAttribute == attribute.RecId
                && x.DisplayValue == value, ct)
            ?? throw new LedgerDimensionException($"'{value}' is not a value of financial dimension '{dimension}'.");
        if (resolved.IsSuspended == NoYes.Yes)
            throw new LedgerDimensionException($"Dimension value '{dimension} {value}' is suspended.");
        if (resolved.IsBlockedForManualEntry == NoYes.Yes)
            throw new LedgerDimensionException($"Dimension value '{dimension} {value}' is blocked for manual entry.");
        return resolved;
    }

    internal static string BuildDisplayValue(string mainAccountId, IReadOnlyList<(string Dimension, string Value)> values)
    {
        var segments = new List<string> { mainAccountId };
        segments.AddRange(values.Select(x => x.Value ?? string.Empty));
        // Trailing blank segments carry no information, so they are not written into the display value.
        while (segments.Count > 1 && segments[^1].Length == 0) segments.RemoveAt(segments.Count - 1);
        return string.Join(SegmentSeparator, segments);
    }

    /// <summary>
    /// D365 also keeps a column per well-known dimension on the combination. Set it when one exists
    /// for this dimension; the value group chain remains the general, authoritative representation.
    /// </summary>
    private static void SetDenormalisedColumn(DimensionAttributeValueCombination combination,
        string dimension, string value, long valueRecId)
    {
        if (!DenormalisedColumns.TryGetValue(dimension, out var columns)) return;
        columns.Id.SetValue(combination, valueRecId);
        columns.Display.SetValue(combination, value);
    }

    private static readonly Dictionary<string, (PropertyInfo Id, PropertyInfo Display)> DenormalisedColumns =
        BuildDenormalisedColumns();

    private static Dictionary<string, (PropertyInfo, PropertyInfo)> BuildDenormalisedColumns()
    {
        var map = new Dictionary<string, (PropertyInfo, PropertyInfo)>(StringComparer.OrdinalIgnoreCase);
        var properties = typeof(DimensionAttributeValueCombination).GetProperties();
        foreach (var id in properties.Where(x => x.PropertyType == typeof(long) && x.CanWrite))
        {
            if (id.Name == nameof(DimensionAttributeValueCombination.MainAccount)) continue;
            var display = properties.FirstOrDefault(x => x.Name == id.Name + "Value"
                && x.PropertyType == typeof(string) && x.CanWrite);
            if (display != null) map[id.Name] = (id, display);
        }
        return map;
    }
}
