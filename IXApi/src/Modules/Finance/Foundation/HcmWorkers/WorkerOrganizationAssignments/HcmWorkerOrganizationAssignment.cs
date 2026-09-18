using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
namespace IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;

public class HcmWorkerOrganizationAssignment : Entity<long>
{
    public long HcmWorkerId { get; set; }
    public long OrganizationUnitId { get; set; }
    public long? PositionId { get; set; }
    public long OrganizationRoleId { get; set; }
    public long? OrganizationHierarchyNodeId { get; set; }
    public byte AssignmentRole { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsPrimary { get; set; } = true;
    public virtual HcmWorker HcmWorker { get; set; } = null!;
    public virtual OrganizationUnit OrganizationUnit { get; set; } = null!;
    public virtual HcmPosition? Position { get; set; }
    public virtual OrganizationRole OrganizationRole { get; set; } = null!;
    public virtual OrganizationHierarchyNode? OrganizationHierarchyNode { get; set; }
}
