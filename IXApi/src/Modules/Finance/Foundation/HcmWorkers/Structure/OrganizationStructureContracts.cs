namespace IAX.IXApi.Modules.Finance.Foundation.Structure;

public sealed record CreateOrganizationUnit(string Code, string Name, byte Type, DateOnly ValidFrom, DateOnly? ValidTo, string? NameAlias = null);
public sealed record UpdateOrganizationUnit(string Code, string Name, byte Type);
public sealed record CreateOrganizationRole(string Code, string Name);
public sealed record CreateOrganizationHierarchy(string Code, string Name, string Purpose);
public sealed record UpdateOrganizationHierarchy(string Code, string Name, string Purpose);
public sealed record CreateOrganizationNode(long HierarchyId, long OrganizationUnitId, long? ParentNodeId, DateOnly ValidFrom, DateOnly? ValidTo);
public sealed record CreatePosition(string Code, string Name, long OrganizationUnitId, long RoleId, DateOnly ValidFrom, DateOnly? ValidTo);
public sealed record UpdatePosition(string Code, string Name, long OrganizationUnitId, long RoleId, DateOnly ValidFrom, DateOnly? ValidTo);
public sealed record AssignWorker(long WorkerId, long PositionId, DateOnly ValidFrom, DateOnly? ValidTo, bool IsPrimary = true, long? OrganizationHierarchyNodeId = null);
public sealed record TransferWorker(long PositionId, DateOnly EffectiveDate, DateOnly? ValidTo);
public sealed record CloseOrganizationPeriod(DateOnly ValidTo);
public sealed record OrganizationRoleInfo(long Id, string Code, string Name);
public sealed record OrganizationHierarchyInfo(long Id, string Code, string Name, string Purpose);
public sealed record OrganizationNodeInfo(long Id, long HierarchyId, long OrganizationUnitId, long? ParentNodeId, DateOnly ValidFrom, DateOnly? ValidTo);
public sealed record PositionInfo(long Id, string Code, string Name, long OrganizationUnitId, long RoleId, DateOnly ValidFrom, DateOnly? ValidTo);

/// <summary>Stable codes: preserve the first four values used by the existing unit seeder.</summary>
public enum OrganizationUnitType : byte
{
    Area = 1, Region = 2, SupervisorZone = 3, Showroom = 4,
    Company = 5, BusinessUnit = 6, Branch = 7, Department = 8, Warehouse = 9
}
