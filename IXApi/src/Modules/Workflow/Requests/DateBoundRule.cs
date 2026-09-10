using System.Globalization;
using System.Text.RegularExpressions;

namespace IAX.IXApi.Modules.Workflow.Requests;

public static class DateBoundRule
{
    public static DateOnly? Resolve(string? expression, DateOnly today)
    {
        var text = (expression ?? "").Trim();
        if (DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fixedDate)) return fixedDate;
        var match = Regex.Match(text, @"^today(?:\s*([+-])\s*([0-9]{1,4})\s*([dmy]))?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!match.Success) return null;
        var offset = match.Groups[2].Success ? int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) : 0;
        if (match.Groups[1].Value == "-") offset = -offset;
        try
        {
            return match.Groups[3].Value.ToLowerInvariant() switch
            {
                "d" => today.AddDays(offset),
                "m" => today.AddMonths(offset),
                "y" => today.AddYears(offset),
                _ => today
            };
        }
        catch (ArgumentOutOfRangeException) { return null; }
    }

    public static bool IsValid(string type, string value, string? expression, DateOnly today)
    {
        if (!Regex.IsMatch(value, @"^\d{4}-\d{2}-\d{2}(?:T(?:[01]\d|2[0-3]):[0-5]\d(?::[0-5]\d(?:\.\d{1,7})?)?)?$")) return false;
        if (!DateOnly.TryParseExact(value[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var actual)) return false;
        var bound = Resolve(expression, today);
        return bound.HasValue && (type.Equals("minDate", StringComparison.OrdinalIgnoreCase) ? actual >= bound.Value : actual <= bound.Value);
    }
}
