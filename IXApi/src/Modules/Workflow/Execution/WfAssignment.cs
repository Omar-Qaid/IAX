using IAX.IXApi.Shared.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace IAX.IXApi.Modules.Workflow.Execution
{
    public class WfAssignment : Entity<long>
    {
        public long RequestId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(RequestId))]
        public virtual IAX.IXApi.Modules.Workflow.Requests.WfRequest Request { get; set; } = null!;

        public long ActivityId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ActivityId))]
        public virtual IAX.IXApi.Modules.Workflow.Activities.WfActivity Activity { get; set; } = null!;

        public long StepId { get; set; }
        public long UserId { get; set; }
        public DateTime AssignDate { get; set; }
        public bool IsFinished { get; set; }
        public DateTime? FinishedDate { get; set; }
        public bool AutoPassing { get; set; }
        public byte AutoPassingHrs { get; set; }
        public bool? Automatically { get; set; }
        public bool Transferred { get; set; }
        public decimal Score { get; set; }
    }
}


