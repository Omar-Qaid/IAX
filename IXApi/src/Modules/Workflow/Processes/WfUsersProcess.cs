using IAX.IXApi.Shared.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

using IAX.IXApi.Modules.Finance.Foundation.Occupations;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;

namespace IAX.IXApi.Modules.Workflow.Processes
{
    public class WfUsersProcess : Entity<long>
    {
        public long ProcessId { get; set; }
        [ForeignKey(nameof(ProcessId))]
        public virtual WfProcess Process { get; set; } = null!;
        public short? OccupationId { get; set; }
        [ForeignKey(nameof(OccupationId))]
        public virtual Occupation? Occupation { get; set; }

        public long? EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))]
        public virtual HcmWorker? Employee { get; set; }
    }
}


