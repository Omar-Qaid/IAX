using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Modules.Workflow.Categories;
using IAX.IXApi.Modules.Workflow.Priorities;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Workflow.Processes
{
    [DataManagement]
    public partial class WfProcess : MasterEntity<long>
    {
        public short CategoryId { get; set; }
        
        [ForeignKey(nameof(CategoryId))]
        public virtual WfCategory Category { get; set; } = null!;

        public decimal Score { get; set; }
        public bool CanRepeat { get; set; }
        public bool MandatoryDocs { get; set; }
        
        public byte PriorityId { get; set; }

        [ForeignKey(nameof(PriorityId))]
        public virtual WfPriority Priority { get; set; } = null!;

        public byte ProcessTypeId { get; set; }
        public bool SysField { get; set; }
        public byte SortOrder { get; set; }
        
        public virtual ICollection<WfUsersProcess> UsersProcesses { get; set; } = new List<WfUsersProcess>();
        
        [NotMapped]
        public List<long>? EmployeeId { get; set; }
        [NotMapped]
        public List<long>? OccupationId { get; set; }
        [NotMapped]
        public List<long>? DepartmentId { get; set; }
    }
}
