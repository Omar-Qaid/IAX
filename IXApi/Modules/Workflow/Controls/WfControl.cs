using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Workflow.Controls
{
    public class WfControl : MasterEntity<byte>
    {
        [StringLength(255)]
        public string ControlType { get; set; } = null!;
        public byte SortOrder { get; set; }
    }
}

