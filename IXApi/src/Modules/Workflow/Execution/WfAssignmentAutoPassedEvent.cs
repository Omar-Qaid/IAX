using IAX.IXApi.Shared.Domain.Events;

namespace IAX.IXApi.Modules.Workflow.Execution
{
    /// <summary>
    /// Raised when a workflow assignment is auto-passed by the SLA sweep. The auto-pass job
    /// only knows it finished an assignment — what should happen next (notify, broadcast,
    /// audit…) is decided by subscribers, keeping the job decoupled from those concerns.
    /// </summary>
    public sealed class WfAssignmentAutoPassedEvent : ISysEvent
    {
        public long AssignmentId { get; init; }
        public long ActivityId { get; init; }
        public long RequestId { get; init; }
        public long UserId { get; init; }
        public int AutoPassingHrs { get; init; }
    }
}
