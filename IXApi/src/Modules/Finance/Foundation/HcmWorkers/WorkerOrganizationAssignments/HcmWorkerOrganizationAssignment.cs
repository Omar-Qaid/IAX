using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Finance.Foundation.Structure;

namespace IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;

public class HcmWorkerOrganizationAssignment : Entity<long>
{
    public long AssignmentId { get; set; }
    public string DataAreaId { get; set; } = "dat";
    public long HcmWorkerId { get; set; }
    public long OrganizationUnitId { get; set; }
    public long? PositionId { get; set; }
    public long? OrganizationRoleId { get; set; }
    public byte AssignmentRole { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsPrimary { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }
    public virtual HcmWorker HcmWorker { get; set; } = null!;
    public virtual OrganizationUnit OrganizationUnit { get; set; } = null!;
    public virtual HcmPosition? Position { get; set; }
    public virtual OrganizationRole? OrganizationRole { get; set; }
}