using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestTransition : Entity<long>
    {
        public long RequestControlId { get; set; }
        public byte ControlId { get; set; }
        public byte OperatorId { get; set; }
        public long VariableId { get; set; }
        [StringLength(255)]
        public string Value { get; set; } = null!;
        public long StepId { get; set; }
    }
}


