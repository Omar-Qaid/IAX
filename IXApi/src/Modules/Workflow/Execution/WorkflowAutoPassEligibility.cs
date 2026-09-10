using System.Linq.Expressions;

namespace IAX.IXApi.Modules.Workflow.Execution;

public static class WorkflowAutoPassEligibility
{
    public static Expression<Func<WfAssignment, bool>> At(DateTime now) => a =>
        !a.IsFinished && a.IsActive && !a.IsDeleted
        && a.Request.IsActive && !a.Request.IsDeleted
        && !a.Request.IsFinished && !a.Request.IsStopped
        && a.AutoPassing && a.AutoPassingHrs > 0
        && a.AssignDate.AddHours(a.AutoPassingHrs) <= now;
}
