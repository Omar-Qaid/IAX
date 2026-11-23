using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;

/// <summary>The startup organization structure, read from <c>Data/baseline-organization.json</c>.</summary>
public sealed record OrganizationBaselineCatalog(
    DateOnly EffectiveFrom,
    OrganizationBaselineCatalog.UnitRow[] Units,
    OrganizationBaselineCatalog.RoleRow[] Roles,
    OrganizationBaselineCatalog.HierarchyRow[] Hierarchies,
    OrganizationBaselineCatalog.PositionRow[] Positions,
    OrganizationBaselineCatalog.AssignmentRow[] Assignments,
    OrganizationBaselineCatalog.ReportingHierarchyRow[] ReportingHierarchies,
    OrganizationBaselineCatalog.ReportingLineRow[] ReportingLines)
{
    public static OrganizationBaselineCatalog Load() => BaselineManifest.Read<OrganizationBaselineCatalog>("baseline-organization.json");

    public sealed record UnitRow(string Code, string Name, string NameAlias, byte Type);
    public sealed record RoleRow(string Code, string Name, string NameAlias);
    public sealed record NodeRow(string Unit, string? Parent);
    public sealed record HierarchyRow(string Code, string Name, string NameAlias, string Purpose, NodeRow[] Nodes);
    public sealed record PositionRow(string Code, string Name, string NameAlias, string Unit, string Role);
    public sealed record AssignmentRow(string Position, string PersonnelNumber);
    public sealed record ReportingHierarchyRow(string Code, string Name, string NameAlias, string Purpose);
    public sealed record ReportingLineRow(string Hierarchy, string Subordinate, string Manager);
}
