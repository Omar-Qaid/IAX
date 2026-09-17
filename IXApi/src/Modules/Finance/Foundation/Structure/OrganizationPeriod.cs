using FluentValidation;

namespace IAX.IXApi.Modules.Finance.Foundation.Structure;

/// <summary>All organization periods are start-inclusive and end-exclusive.</summary>
public static class OrganizationPeriod
{
    public static void Validate(DateOnly start, DateOnly? end)
    {
        Require(end == null || end > start, "ValidTo must be later than ValidFrom (exclusive end date).");
    }

    public static bool Overlaps(DateOnly start, DateOnly? end, DateOnly otherStart, DateOnly? otherEnd) =>
        (end == null || otherStart < end) && (otherEnd == null || start < otherEnd);

    public static bool Contains(DateOnly start, DateOnly? end, DateOnly childStart, DateOnly? childEnd) =>
        childStart >= start && (end == null || (childEnd != null && childEnd <= end));

    public static void Require(bool condition, string message)
    {
        if (!condition) throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Organization", message) });
    }

    public static string Text(string? value, int maxLength, string field)
    {
        var text = value?.Trim();
        Require(!string.IsNullOrWhiteSpace(text) && text.Length <= maxLength, $"{field} is required and must not exceed {maxLength} characters.");
        return text!;
    }
}
