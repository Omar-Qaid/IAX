using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Workflow.Controls;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityControl : MasterEntity<long>
    {
        public long ActivityId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ActivityId))]
        public virtual WfActivity Activity { get; set; } = null!;
        public long ProcessId { get; set; }
        public byte ControlId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ControlId))]
        public virtual  WfControl Control { get; set; } = null!;
        public decimal Score { get; set; }
        public byte SortOrder { get; set; }
        public string? ValidationRules { get; set; }  // as  xml 
        public string? ExtendedProperties { get; set; }  // as  xml 

    }
}

