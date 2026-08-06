using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Application.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace IAX.IXApi.Modules.Workflow.Categories
{
    [DataManagement]
    public class WfCategory : MasterEntity<short>
    {
        public bool SysField { get; set; }
        public byte SortOrder { get; set; }
    }
}

