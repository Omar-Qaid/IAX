using IAX.IXApi.Modules.Finance.Foundation.Departments;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.Occupations;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;

public class HcmWorkerOrganizationAssignmentV1 : Entity<long>
{
    public long HcmWorkerId { get; set; }
    public long HcmManagerWorkerId { get; set; }
    public short? DepartmentId { get; set; }
    public short? OccupationId { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsPrimary { get; set; } = true;
    public virtual HcmWorker HcmWorker { get; set; } = null!;
    public virtual HcmWorker HcmManager { get; set; } = null!;
    public virtual HcmDepartment? Department { get; set; }
    public virtual HcmOccupation? Occupation { get; set; }
}
