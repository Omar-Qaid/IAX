# Organization units and worker assignments

`OrganizationUnit` maps to `OrganizationUnits`. Types: 1 Area, 2 Region,
3 SupervisorZone, 4 Showroom, 5 Company, 6 BusinessUnit, 7 Branch, 8 Department,
9 Warehouse. Codes are unique per company; legacy parent links are optional.
See [Shared Organization structure](../Structure/README.md) for the new shared
directory, positions, management APIs and dated multi-purpose hierarchies.

`HcmWorkerOrganizationAssignment` maps to `HcmWorkerOrganizationAssignments`.
`HcmWorkerId` references the existing `HcmWorker.RecId`; no Employees table is added.
Roles: 1 Seller, 2 Supervisor, 3 RegionManager, 4 AreaManager, 5 NormalWorker.
Close an old assignment with exclusive ValidTo (leave IsActive=true for history), then insert a new row
when a worker transfers. Do not rewrite the organizational unit of historical assignments.

The nullable WfRequest fields are RequestForType (1 HcmWorker, 2 OrganizationUnit),
RequestForHcmWorkerId, OrganizationUnitId and HcmWorkerAssignmentId.
The existing EmployeeId field remains compatible with current callers.
These changes add persistence only; request DTOs and approval resolution are not changed.

OrganizationUnitSeeder creates the standalone example hierarchy directly:
AREA-W (Western Area) -> REG-JED (Jeddah Region) -> SUP-NJ (North Jeddah),
with SH-A and SH-B as child units of SUP-NJ. It resolves generated parent IDs
and skips existing codes without changing their values or hierarchy.
It does not read Showrooms or HcmWorker.ShowroomId and does not seed worker
assignments. Existing assignments are preserved. Worker assignments require
explicit HcmWorker IDs and effective dates. The legacy Showroom feature remains
unchanged. Apply the EF schema migration before running the seeder.
