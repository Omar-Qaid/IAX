using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Workflow.Variables;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityMappingVariable : Entity<long>
    {
        public long ActivityControlId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ActivityControlId))]
        public virtual WfActivityControl ActivityControl { get; set; } = null!;

        public long VariableId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(VariableId))]
        public virtual WfVariable Variable { get; set; } = null!;

        public byte VariableOrder { get; set; }
    }
}


