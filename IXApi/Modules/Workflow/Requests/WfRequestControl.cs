using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Workflow.Controls;
using IAX.IXApi.Modules.Workflow.Processes;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestControl: MasterEntity<long>
    {
        public long ProcessId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ProcessId))]
        public virtual WfProcess Process { get; set; } = null!;
        public byte ControlId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ControlId))]
        public virtual WfControl Control { get; set; } = null!;
        public decimal Score { get; set; }
        public byte SortOrder { get; set; }
        public string? ValidationRules { get; set; }  // as  xml 
        public string? ExtendedProperties { get; set; }  // as  xml 
    }
}
