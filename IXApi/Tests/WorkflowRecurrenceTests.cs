using IAX.IXApi.Modules.Workflow.Scheduling;
using Xunit;

namespace IXApi.Tests;

public class WorkflowRecurrenceTests
{
    [Theory]
    [InlineData("daily", "2026-09-11T08:00:00")]
    [InlineData("weekly", "2026-09-17T08:00:00")]
    [InlineData("monthly", "2026-10-10T08:00:00")]
    [InlineData("yearly", "2027-09-10T08:00:00")]
    public void ReturnsNextOccurrenceAfterCompletedSlot(string frequency, string expected)
    {
        var start = new DateTime(2026, 9, 10, 8, 0, 0);
        Assert.Equal(DateTime.Parse(expected), WorkflowRecurrence.NextUtc(start, "UTC", frequency, start));
    }

    [Fact]
    public void MonthEndStaysAnchoredAfterFebruary()
    {
        Assert.Equal(new DateTime(2026, 3, 31, 8, 0, 0), WorkflowRecurrence.NextUtc(
            new DateTime(2026, 1, 31, 8, 0, 0), "UTC", "monthly", new DateTime(2026, 2, 28, 8, 0, 0)));
    }

    [Fact]
    public void MissedOccurrencesAreSkipped()
    {
        Assert.Equal(new DateTime(2026, 9, 11), WorkflowRecurrence.NextUtc(
            new DateTime(2020, 1, 1), "UTC", "daily", new DateTime(2026, 9, 10, 12, 0, 0)));
    }

    [Fact]
    public void UnknownFrequencyIsRejected() => Assert.Throws<ArgumentException>(() =>
        WorkflowRecurrence.NextUtc(DateTime.Today, "UTC", "hourly", DateTime.UtcNow));
}
