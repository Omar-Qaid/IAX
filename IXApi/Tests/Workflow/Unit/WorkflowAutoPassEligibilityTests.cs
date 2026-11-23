using IAX.IXApi.Modules.Workflow.Execution;
using IAX.IXApi.Modules.Workflow.Requests;
using Xunit;

namespace IAX.IXApi.Tests;

public class WorkflowAutoPassEligibilityTests
{
    [Theory]
    [InlineData(1, false)]
    [InlineData(59, false)]
    [InlineData(60, true)]
    public void Waits_for_full_configured_duration(int elapsedMinutes, bool expected)
    {
        var assignment = Assignment();
        Assert.Equal(expected, WorkflowAutoPassEligibility.At(assignment.AssignDate.AddMinutes(elapsedMinutes)).Compile()(assignment));
    }

    [Theory]
    [InlineData("stopped")]
    [InlineData("finishedRequest")]
    [InlineData("finishedTask")]
    [InlineData("disabled")]
    [InlineData("deleted")]
    public void Does_not_advance_ineligible_work(string state)
    {
        var assignment = Assignment();
        if (state == "stopped") assignment.Request.IsStopped = true;
        if (state == "finishedRequest") assignment.Request.IsFinished = true;
        if (state == "finishedTask") assignment.IsFinished = true;
        if (state == "disabled") assignment.AutoPassing = false;
        if (state == "deleted") assignment.IsDeleted = true;
        Assert.False(WorkflowAutoPassEligibility.At(assignment.AssignDate.AddDays(1)).Compile()(assignment));
    }

    private static WfAssignment Assignment() => new()
    {
        AssignDate = new DateTime(2026, 9, 10, 10, 59, 0, DateTimeKind.Utc),
        AutoPassing = true, AutoPassingHrs = 1, IsActive = true,
        Request = new WfRequest { IsActive = true }
    };
}
