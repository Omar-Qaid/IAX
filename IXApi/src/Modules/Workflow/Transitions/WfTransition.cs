using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Processes;
using IAX.IXApi.Modules.Workflow.Variables;

namespace IAX.IXApi.Modules.Workflow.Transitions
{
    public class WfTransition : Entity<long>
    {
        public long ProcessId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ProcessId))]
        public virtual WfProcess Process { get; set; } = null!;

        public long? ActivityId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ActivityId))]
        public virtual WfActivity? Activity { get; set; }

        public long? RequestControlId { get; set; }
        public long VariableId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(VariableId))]
        public virtual WfVariable Variable { get; set; } = null!;

        public byte OperatorId { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string Value { get; set; } = null!;
        public long StepId { get; set; }
        public byte SortOrder { get; set; }

    }
}


