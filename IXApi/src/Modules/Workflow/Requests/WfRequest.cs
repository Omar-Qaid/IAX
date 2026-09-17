using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Workflow.Processes;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;

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
        public virtual HcmWorker? Employee { get; set; }
        public string RequestDetails { get; set; } = null!;
        public bool IsFinished { get; set; }
        public DateTime? FinishedDate { get; set; }
        public bool IsStopped { get; set; }
        public DateTime? StoppedDate { get; set; }
        public decimal Score { get; set; }
        public decimal Progress { get; set; }
        public string? Notes { get; set; }
        public byte? RequestForType { get; set; }
        public long? RequestForHcmWorkerId { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? HcmWorkerAssignmentId { get; set; }
        public virtual HcmWorker? RequestForHcmWorker { get; set; }
        public virtual OrganizationUnit? OrganizationUnit { get; set; }
        public virtual HcmWorkerOrganizationAssignment? HcmWorkerAssignment { get; set; }
        
    }
}

