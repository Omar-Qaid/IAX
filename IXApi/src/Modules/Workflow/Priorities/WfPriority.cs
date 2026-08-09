using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Application.Attributes;

namespace IAX.IXApi.Modules.Workflow.Priorities
{
    [DataManagement]
    public class WfPriority : MasterEntity<byte>
    {
        public byte SortOrder { get; set; }
    }
}

