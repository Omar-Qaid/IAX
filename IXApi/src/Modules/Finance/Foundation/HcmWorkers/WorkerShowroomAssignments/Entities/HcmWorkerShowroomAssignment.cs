using IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments;

public class HcmWorkerShowroomAssignment : Entity<long>
{
    public long HcmWorkerId { get; set; }
    public long HcmShowroomId { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsPrimary { get; set; } = true;

    public virtual HcmWorker HcmWorker { get; set; } = null!;
    public virtual HcmShowroom HcmShowroom { get; set; } = null!;
}
