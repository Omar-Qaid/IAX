using IAX.IXApi.Modules.Finance.Foundation.Structure;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.HumanResources;

internal static class HcmWorkerOrganizationAssignmentDemoData
{
    internal static readonly (string PersonnelNumber, string PositionCode)[] Assignments =
    [
        ("DEMO-DIRECTOR", "AREA-W-MGR"), ("DEMO-SALES-MGR", "REG-JED-MGR"),
        ("DEMO-SUPERVISOR", "SUP-NJ-SUP"), ("DEMO-SELLER-A", "SH-A-SELLER-01"),
        ("DEMO-SELLER-B", "SH-B-SELLER-01"), ("DEMO-FIN-MGR", "ORG-FIN-MGR"),
        ("DEMO-ACCOUNTANT", "ORG-FIN-AR-EMP-01")
    ];
    internal static HcmWorkerOrganizationAssignment Create(long worker, HcmPosition position) => new()
    {
        DataAreaId = "dat", HcmWorkerId = worker, PositionId = position.RecId,
        OrganizationUnitId = position.OrganizationUnitId, OrganizationRoleId = position.RoleId,
        AssignmentRole = 1, IsPrimary = true, IsActive = true,
        ValidFrom = new DateOnly(2026, 1, 1), CreatedBy = "sys", OwnerAccountId = "sys"
    };
}
