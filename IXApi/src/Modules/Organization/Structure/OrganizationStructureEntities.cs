using IAX.IXApi.Modules.Organization.OrganizationUnits;

namespace IAX.IXApi.Modules.Organization.Structure;

public sealed class OrganizationRole : MasterEntity<long>
{
    
}

public sealed class OrganizationHierarchy : MasterEntity<long>
{
    public string Purpose { get; set; } = string.Empty;
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
}

public sealed class HcmPosition : MasterEntity<long>
{
    public long OrganizationUnitId { get; set; }
    public long RoleId { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public OrganizationUnit OrganizationUnit { get; set; } = null!;
    public OrganizationRole Role { get; set; } = null!;
}
