using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Transitions
{
    public class WfTransitionDto : EntityDto<long>
    {
        public long ProcessId { get; set; }
        public long? ActivityId { get; set; }
        public long? RequestControlId { get; set; }
        public long VariableId { get; set; }
        public byte OperatorId { get; set; }
        public string Value { get; set; } = null!;
        public long StepId { get; set; }
        public byte SortOrder { get; set; }
    }
}
