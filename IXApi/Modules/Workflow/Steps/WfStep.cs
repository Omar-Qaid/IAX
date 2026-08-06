using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Workflow.Processes;

namespace IAX.IXApi.Modules.Workflow.Steps
{
    [DataManagement]
    public class WfStep : MasterEntity<long>
    {
        public long ProcessId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ProcessId))]
        public virtual WfProcess Process { get; set; } = null!;
        public byte SortOrder { get; set; }
        public decimal Score { get; set; }
        public byte AutoPassingHrs { get; set; }
        public bool AllMandatory { get; set; }
        public bool SysField { get; set; }
    }
}
