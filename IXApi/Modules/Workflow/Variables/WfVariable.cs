using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Workflow.Processes;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Workflow.Variables
{
    public class WfVariable : MasterEntity<long>
    {
        public byte DataTypeId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(DataTypeId))]
        public virtual WfDataType DataType { get; set; } = null!;

        public long ProcessId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ProcessId))]
        public virtual WfProcess Process { get; set; } = null!;

        public byte SortOrder { get; set; }
    }
}

