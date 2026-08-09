using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Workflow.Execution
{
    public class WfTransfer : Entity<long>
    {
        public long AssignmentId { get; set; }
        public long FromUserId { get; set; }
        public long ToUserId { get; set; }
        public DateTime TransferDate { get; set; }
        public string? Reason { get; set; }
    }
}

