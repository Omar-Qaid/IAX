using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Workflow.Execution;
using System;

namespace IAX.IXApi.Modules.Workflow.Processes
{
    public class WfProcessData:Entity<long>
    {
        public long? AssignmentID { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(AssignmentID))]
        public virtual WfAssignment? Assignment { get; set; }

        public DateTime FinishDate { get; set; }
        public string ActivityDetails { get; set; } = null!;
        public string? ExtendedProperties { get; set; }
        public long? AttachmentId { get; set; }
    }
}

