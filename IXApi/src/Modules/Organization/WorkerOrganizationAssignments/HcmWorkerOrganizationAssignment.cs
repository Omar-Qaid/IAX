using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Organization.OrganizationUnits;

namespace IAX.IXApi.Modules.Organization.WorkerOrganizationAssignments;

public class HcmWorkerOrganizationAssignment : IMultiCompany
{
    public string DataAreaId { get; set; } = "dat";
    public long? PositionId { get; set; }
    public virtual IAX.IXApi.Modules.Organization.Structure.HcmPosition? Position { get; set; }
    public long AssignmentId { get; set; }
    public long HcmWorkerId { get; set; }
    public long OrganizationUnitId { get; set; }
    public byte AssignmentRole { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsPrimary { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }
    public virtual HcmWorker HcmWorker { get; set; } = null!;
    public virtual OrganizationUnit OrganizationUnit { get; set; } = null!;
}
