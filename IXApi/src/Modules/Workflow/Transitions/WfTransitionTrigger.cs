using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Workflow.Transitions
{
    public class WfTransitionTrigger : Entity<long>
    {
        public long TransitionId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(TransitionId))]
        public virtual WfTransition Transition { get; set; } = null!;
        public byte TriggerOrder { get; set; }
        public bool Activated { get; set; }
        public byte OperatorId { get; set; }
        public byte TriggerType { get; set; } = 1;
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string TableName { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string? Expression { get; set; }
    }
}


