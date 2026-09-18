using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;

namespace IAX.IXApi.Modules.Finance.Foundation.Structure;

public sealed class OrganizationRole : MasterEntity<long>
{
    public ICollection<HcmPosition> Positions { get; set; } = new List<HcmPosition>();
    public ICollection<IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments.HcmWorkerOrganizationAssignment> WorkerAssignments { get; set; } = new List<IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments.HcmWorkerOrganizationAssignment>();
}

public sealed class OrganizationHierarchy : MasterEntity<long>
{
    public string Purpose { get; set; } = string.Empty;
    public ICollection<OrganizationHierarchyNode> Nodes { get; set; } = new List<OrganizationHierarchyNode>();
}

public sealed class OrganizationHierarchyNode : Entity<long>
{
    public long HierarchyId { get; set; }
    public long OrganizationUnitId { get; set; }
    public long? ParentNodeId { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public OrganizationHierarchy Hierarchy { get; set; } = null!;
    public OrganizationUnit OrganizationUnit { get; set; } = null!;
    public OrganizationHierarchyNode? ParentNode { get; set; }
    public ICollection<OrganizationHierarchyNode> Children { get; set; } = new List<OrganizationHierarchyNode>();
    public ICollection<IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments.HcmWorkerOrganizationAssignment> WorkerAssignments { get; set; } = new List<IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments.HcmWorkerOrganizationAssignment>();
}

public sealed class HcmPosition : MasterEntity<long>
{
    public long OrganizationUnitId { get; set; }
    public long RoleId { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public OrganizationUnit OrganizationUnit { get; set; } = null!;
    public OrganizationRole Role { get; set; } = null!;
    public ICollection<IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments.HcmWorkerOrganizationAssignment> WorkerAssignments { get; set; } = new List<IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments.HcmWorkerOrganizationAssignment>();
}
