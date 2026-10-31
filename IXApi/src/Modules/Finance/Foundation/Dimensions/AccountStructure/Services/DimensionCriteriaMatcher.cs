using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;

/// <summary>
/// Parses and matches account structure allowed-value criteria, exactly as the account structure
/// page documents them. Shared by activation, which needs to know whether two structures overlap,
/// and by posting validation, which needs to know whether a value is allowed.
///
/// The rules that matter, and that are easy to get wrong:
///
/// - **Values are compared as strings, never as numbers.** `1000000` falls inside the string range
///   `100000..399999`, because comparison runs character by character from the left. Prefix shorter
///   values with zeros if that is not what you want.
/// - Criteria are separated by a **semicolon**. A comma is a valid character inside a dimension
///   value, so it never separates criteria.
/// - `*` allows every **non-blank** value. Blank is allowed separately, by including `""`.
/// - Wildcards are not valid inside a range: `100*..399*` matches nothing useful. Use
///   `500*` (begins with), `*500` (ends with) or `*500*` (is like).
/// </summary>
public static class DimensionCriteriaMatcher
{
    public const string RangeSeparator = "..";
    private const char CriteriaSeparator = ';';
    private const char Wildcard = '*';

    /// <summary>A single parsed criterion: a range, or a wildcard expression, or blank.</summary>
    public sealed record Criterion(string RangeFrom, string RangeTo, string WildCardString, bool IsBlank)
    {
        public bool IsRange => RangeFrom.Length > 0 || RangeTo.Length > 0;
    }

    /// <summary>
    /// Parses what a user writes in an allowed-values cell, such as <c>100000..399999</c>,
    /// <c>*;""</c> or <c>ADMIN;SALES;SERVICE</c>. A <c>""</c> token means blank is allowed, which
    /// the caller turns into the node's IsOptional rather than a criterion of its own.
    /// </summary>
    public static IReadOnlyList<Criterion> Parse(string allowedValues)
    {
        var parsed = new List<Criterion>();
        foreach (var raw in (allowedValues ?? string.Empty).Split(CriteriaSeparator))
        {
            var token = raw.Trim();
            if (token.Length == 0) continue;
            if (token is "\"\"" or "''")
            {
                parsed.Add(new Criterion(string.Empty, string.Empty, string.Empty, IsBlank: true));
                continue;
            }
            var separator = token.IndexOf(RangeSeparator, StringComparison.Ordinal);
            if (separator > 0)
            {
                var from = token[..separator].Trim();
                var to = token[(separator + RangeSeparator.Length)..].Trim();
                parsed.Add(new Criterion(from, to, string.Empty, IsBlank: false));
                continue;
            }
            parsed.Add(new Criterion(string.Empty, string.Empty, token, IsBlank: false));
        }
        return parsed;
    }

    /// <summary>
    /// Whether a segment value satisfies a node's criteria. A blank value is allowed only when the
    /// segment is optional — the criteria themselves never allow blank, because `*` does not.
    /// </summary>
    public static bool Allows(IEnumerable<DimensionConstraintNodeCriteria> criteria, NoYes isOptional, string value)
    {
        if (string.IsNullOrEmpty(value)) return isOptional == NoYes.Yes;
        return criteria.Any(x => Matches(x, value));
    }

    private static bool Matches(DimensionConstraintNodeCriteria criterion, string value)
    {
        if (criterion.RangeFrom.Length > 0 || criterion.RangeTo.Length > 0)
            return InRange(value, criterion.RangeFrom, criterion.RangeTo,
                criterion.IsFromOpen == NoYes.Yes, criterion.IsToOpen == NoYes.Yes);
        return MatchesWildcard(value, criterion.WildCardString);
    }

    /// <summary>String range comparison, with each bound optional and possibly exclusive.</summary>
    public static bool InRange(string value, string from, string to, bool fromExclusive, bool toExclusive)
    {
        if (from.Length > 0)
        {
            var compared = string.CompareOrdinal(value, from);
            if (compared < 0 || (fromExclusive && compared == 0)) return false;
        }
        if (to.Length > 0)
        {
            var compared = string.CompareOrdinal(value, to);
            if (compared > 0 || (toExclusive && compared == 0)) return false;
        }
        return true;
    }

    /// <summary>`*` for any non-blank value, `abc*`, `*abc`, `*abc*`, or an exact value.</summary>
    public static bool MatchesWildcard(string value, string pattern)
    {
        if (pattern.Length == 0) return false;
        if (pattern == "*") return value.Length > 0;
        var startsWildcard = pattern[0] == Wildcard;
        var endsWildcard = pattern.Length > 1 && pattern[^1] == Wildcard;
        var core = pattern.Trim(Wildcard);
        if (core.Length == 0) return value.Length > 0;
        if (startsWildcard && endsWildcard) return value.Contains(core, StringComparison.Ordinal);
        if (startsWildcard) return value.EndsWith(core, StringComparison.Ordinal);
        if (endsWildcard) return value.StartsWith(core, StringComparison.Ordinal);
        return string.Equals(value, core, StringComparison.Ordinal);
    }

    /// <summary>
    /// Whether two criteria sets could both accept the same value, which is the overlap that makes
    /// structure selection ambiguous. Where disjointness cannot be established — because a wildcard
    /// is involved — this reports an overlap rather than assuming none: refusing an activation is
    /// recoverable, while an ambiguous structure silently misroutes every posting.
    /// </summary>
    public static bool MayOverlap(IReadOnlyList<Criterion> left, IReadOnlyList<Criterion> right) =>
        left.Where(x => !x.IsBlank).Any(a => right.Where(x => !x.IsBlank).Any(b => MayOverlap(a, b)));

    private static bool MayOverlap(Criterion left, Criterion right)
    {
        if (left.IsRange && right.IsRange) return RangesOverlap(left, right);
        if (left.IsRange) return ValueMayFallInRange(right.WildCardString, left);
        if (right.IsRange) return ValueMayFallInRange(left.WildCardString, right);
        return WildcardsMayOverlap(left.WildCardString, right.WildCardString);
    }

    private static bool RangesOverlap(Criterion left, Criterion right) =>
        (left.RangeTo.Length == 0 || right.RangeFrom.Length == 0
            || string.CompareOrdinal(right.RangeFrom, left.RangeTo) <= 0)
        && (right.RangeTo.Length == 0 || left.RangeFrom.Length == 0
            || string.CompareOrdinal(left.RangeFrom, right.RangeTo) <= 0);

    private static bool ValueMayFallInRange(string pattern, Criterion range)
    {
        // A wildcard stands for values this check cannot enumerate, so it is treated as reaching
        // into the range.
        if (pattern.Length == 0 || pattern.Contains(Wildcard)) return true;
        return InRange(pattern, range.RangeFrom, range.RangeTo,
            fromExclusive: false, toExclusive: false);
    }

    private static bool WildcardsMayOverlap(string left, string right)
    {
        if (left.Contains(Wildcard) || right.Contains(Wildcard)) return true;
        return string.Equals(left, right, StringComparison.Ordinal);
    }
}
