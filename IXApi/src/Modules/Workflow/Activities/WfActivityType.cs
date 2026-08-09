using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using System.ComponentModel.DataAnnotations;


namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityType : MasterEntity<byte>
    {

        public byte SortOrder { get; set; }
    }
}


