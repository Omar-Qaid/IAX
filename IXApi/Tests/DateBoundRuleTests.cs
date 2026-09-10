using IAX.IXApi.Modules.Workflow.Requests;
using Xunit;

namespace IAX.IXApi.Tests;

public class DateBoundRuleTests
{
    [Theory]
    [InlineData("today", "2024-02-29")]
    [InlineData("today+1y", "2025-02-28")]
    [InlineData("today-30d", "2024-01-30")]
    [InlineData("today+6m", "2024-08-29")]
    [InlineData("2025-02-29", null)]
    [InlineData("today+10000y", null)]
    public void Resolves_bounds_without_rolling_invalid_calendar_dates(string expression, string? expected)
    {
        Assert.Equal(expected, DateBoundRule.Resolve(expression, new DateOnly(2024, 2, 29))?.ToString("yyyy-MM-dd"));
    }

    [Fact]
    public void Enforces_inclusive_bounds_and_rejects_invalid_manual_values()
    {
        var today = new DateOnly(2024, 2, 29);
        Assert.True(DateBoundRule.IsValid("maxDate", "2025-02-28T23:59:59", "today+1y", today));
        Assert.False(DateBoundRule.IsValid("maxDate", "2025-03-01", "today+1y", today));
        Assert.False(DateBoundRule.IsValid("minDate", "2024-02-28", "today", today));
        Assert.False(DateBoundRule.IsValid("minDate", "2024-02-29T99:99", "today", today));
        Assert.Equal(new DateOnly(2024, 2, 29), DateBoundRule.Resolve("today+1m", new DateOnly(2024, 1, 31)));
    }
}
