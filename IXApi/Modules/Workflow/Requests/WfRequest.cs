using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Organization.Attachments;

using IAX.IXApi.Modules.Workflow.Processes;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequest : LookupEntity<long>
    {
        public DateTime RequestDate { get; set; }
        public long ProcessId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ProcessId))]
        public virtual WfProcess Process { get; set; } = null!;
        public long? EmployeeId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(EmployeeId))]
        public virtual IAX.IXApi.Modules.Organization.Employees.Entities.HcmWorker? Employee { get; set; }
        public string RequestDetails { get; set; } = null!;
        public bool IsFinished { get; set; }
        public DateTime? FinishedDate { get; set; }
        public bool IsStopped { get; set; }
        public DateTime? StoppedDate { get; set; }
        public decimal Score { get; set; }
        public decimal Progress { get; set; }
        public string? Notes { get; set; }
        
        public long? AttachmentId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(AttachmentId))]
        public virtual OrgAttachment? Attachment { get; set; }
    }
}

