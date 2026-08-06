using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestMappingVariable : Entity<long>
    {
        public long RequestControlId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(RequestControlId))]
        public virtual WfRequestControl RequestControl { get; set; } = null!;

        public long VariableId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(VariableId))]
        public virtual IAX.IXApi.Modules.Workflow.Variables.WfVariable Variable { get; set; } = null!;

        public byte SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}

