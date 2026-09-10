using System.Globalization;
using System.Text.Json;

namespace IAX.IXApi.Modules.Workflow.Execution;

public static class WorkflowConditionEvaluator
{
    public static bool Evaluate(string? typeCode, string? operatorCode, string? actual, string expected)
    {
        var type = typeCode?.Trim().ToUpperInvariant() ?? string.Empty;
        if (type is not ("INT" or "DEC" or "STR" or "DT" or "BOOL"))
            throw new InvalidOperationException($"Unsupported workflow data type '{typeCode}'.");
        var op = operatorCode?.Trim().ToUpperInvariant() switch
        {
            "EQ" or "=" => "=", "NEQ" or "!=" or "<>" => "!=",
            "GT" or ">" => ">", "LT" or "<" => "<", "GTE" or ">=" => ">=", "LTE" or "<=" => "<=",
            "CONTAINS" => "contains", "ISEMPTY" => "isEmpty", "BETWEEN" => "between",
            _ => throw new InvalidOperationException($"Unsupported workflow operator '{operatorCode}'.")
        };
        if (op == "isEmpty") return string.IsNullOrEmpty(actual);
        if (op == "contains")
        {
            if (type != "STR") throw new InvalidOperationException("Contains requires a String variable.");
            return (actual ?? string.Empty).Contains(expected, StringComparison.OrdinalIgnoreCase);
        }
        if (type is "STR" or "BOOL" && op is not ("=" or "!="))
            throw new InvalidOperationException("String and Boolean variables support equality and inequality only.");
        object Parse(string value) => type switch
        {
            "STR" => value,
            "INT" => long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture),
            "DEC" => decimal.Parse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
            "BOOL" => value is "true" ? true : value is "false" ? false : throw new FormatException("Boolean values must be true or false."),
            "DT" => ParseDate(value),
            _ => throw new InvalidOperationException()
        };
        int Compare(object left, object right) => type == "STR"
            ? StringComparer.OrdinalIgnoreCase.Compare((string)left, (string)right)
            : ((IComparable)left).CompareTo(right);
        if (op == "between")
        {
            var bounds = JsonSerializer.Deserialize<string[]>(expected);
            if (bounds?.Length != 2) throw new FormatException("Between requires a JSON array of two string operands.");
            var low = Parse(bounds[0]); var high = Parse(bounds[1]);
            if (Compare(low, high) > 0) throw new FormatException("Between minimum exceeds maximum.");
            if (string.IsNullOrEmpty(actual)) return false;
            var value = Parse(actual);
            return Compare(value, low) >= 0 && Compare(value, high) <= 0;
        }
        var operand = Parse(expected);
        if (string.IsNullOrEmpty(actual) && type != "STR") return false;
        var comparison = Compare(Parse(actual ?? string.Empty), operand);
        return op switch { "=" => comparison == 0, "!=" => comparison != 0, ">" => comparison > 0, "<" => comparison < 0, ">=" => comparison >= 0, "<=" => comparison <= 0, _ => false };
    }

    private static DateTimeOffset ParseDate(string value)
    {
        if (DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        if (!value.Contains('T') || !(value.EndsWith('Z') || value.LastIndexOf('+') > 10 || value.LastIndexOf('-') > 10))
            throw new FormatException("Date/time operands require ISO dates or timestamps with an explicit offset.");
        return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.None).ToUniversalTime();
    }
}
