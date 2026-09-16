# Shared Organization structure

Organization provides the directory for all modules. It does not own sales,
inventory, financial posting, HR policy or workflow approval decisions.

## Ownership and compatibility

- HcmWorker remains the worker record. HcmPosition is a specific seat in a unit;
  OrganizationRole describes its business responsibility, not an Identity role.
- OrganizationUnit has a company, type and effective period. Stable type values:
  1 Area, 2 Region, 3 SupervisorZone, 4 Showroom, 5 Company, 6 BusinessUnit,
  7 Branch, 8 Department, 9 Warehouse.
- OrganizationHierarchy has a configurable purpose. Its dated nodes define
  membership and parentage. Multiple hierarchies can contain the same unit.
- Existing ParentOrganizationUnitId is retained for legacy data. New APIs do not
  read it when resolving hierarchy ancestry. The standalone example seeder does
  not automatically create new hierarchy nodes or assignments.
- Existing HcmWorkerOrganizationAssignment IDs and optional WfRequest references
  remain valid. Nullable PositionId allows existing role-byte assignments to be
  retained. New writes require a position and derive the unit from that position.
  AssignmentRole is legacy metadata; position-based consumers use RoleCode.
- Existing Showroom, Department, worker manager links and transaction APIs remain
  operational. New units do not automatically replace those records.

## Effective dates and history

All periods are `[ValidFrom, ValidTo)`, with null meaning no end. A transfer effective
2026-09-16 closes the previous assignment at 2026-09-16 and starts a new assignment
that day. Closing does not set IsActive=false: historical queries still see the row.
IsActive is an administrative availability flag, not an effective-date substitute.

A position has at most one occupant at a time. A worker has at most one primary
assignment per company at a time, but may hold additional non-primary positions.
Serializably isolated management transactions protect overlap checks. A failed
command rolls back and clears tracked changes. Management commands require a clean
unit of work; query contracts may be used inside a consuming module's transaction.

Position unit and role, and node parentage, are immutable through these APIs.
To reorganize, close descendants before their parents, and create replacement
nodes from the effective date. To move a seat, close/transfer occupants, close the
old position, and create a new position code. No destructive delete API is exposed.
These commands support effective-dated changes, not a full historical correction
or backdated transaction restatement system.

## Shared consumption

Inject `IAX.IXApi.Shared.Application.Organization.IOrganizationDirectory` in any
module. Its implementation is registered by AddOrganizationModule. It returns
DTOs and needs no module-to-Workflow dependency or EF navigation exposure.

```csharp
// Finance: find responsibility on the posting date.
var assignments = await directory.GetWorkerAssignmentsAsync(workerId, postingDate, ct);

// Sales / Inventory: report the operational ancestry of a showroom / warehouse.
var path = await directory.GetAncestorsAsync(hierarchyId, unitId, transactionDate, ct);

// HR / Administration / Workflow: find people holding a business responsibility.
var occupants = await directory.GetRoleOccupantsAsync(
    hierarchyId, unitId, "REGION_MANAGER", effectiveDate, ct);
```

Ancestry is returned starting with the requested unit, then its parents. An absent
membership returns an empty path. Role lookup searches that entire path and returns
all matching assignments; zero means vacant/unconfigured, more than one is not
silently reduced to one person. The consumer chooses hierarchy, date, fallback and
selection policy. Persist the selected IDs/context in transactions when historical
reproducibility is required. Do not infer approvers or security grants here.

Company always comes from ICompanyExecutionContext. HTTP inputs cannot override it.
In-process callers remain responsible for their business operation's permission;
the directory enforces company boundaries, not module-specific permissions.

## Management API

Base route: `/api/v1/organization-structure`. Requires authentication and
`Organization.Structure.View/Create/Edit` according to the HTTP method. Permission
definitions are included in IdentitySeeder; no ordinary role is automatically granted
access. Responses are DTO arrays for reads and generated record IDs for commands.
All dated GETs require `?asOf=YYYY-MM-DD`.

| Operation | Route |
| --- | --- |
| List/create units | GET/POST `units` |
| Close unit | PUT `units/{id}/close` |
| List/create business roles | GET/POST `roles` |
| List/create hierarchies | GET/POST `hierarchies` |
| List effective hierarchy nodes | GET `hierarchies/{id}/nodes` |
| Add/close hierarchy node | POST `nodes`; PUT `nodes/{id}/close` |
| List/create positions | GET/POST `positions` |
| Close position | PUT `positions/{id}/close` |
| Assign worker | POST `assignments` |
| Transfer worker | PUT `assignments/{id}/transfer` |
| Close assignment | PUT `assignments/{id}/close` |
| Worker's dated assignments | GET `workers/{id}/assignments` |
| Unit's dated ancestry | GET `hierarchies/{hierarchyId}/units/{unitId}/ancestors` |
| Dated responsibility lookup | GET `hierarchies/{hierarchyId}/units/{unitId}/role-occupants?roleCode=REGION_MANAGER` |

Create a role:
```json
{ "code": "REGION_MANAGER", "name": "Region Manager" }
```

Create a position (use IDs returned by your company-scoped unit/role APIs):
```json
{ "code": "JED-RM-01", "name": "Jeddah Region Manager", "organizationUnitId": 12,
  "roleId": 3, "validFrom": "2026-01-01", "validTo": null }
```

Assign an existing worker:
```json
{ "workerId": 42, "positionId": 7, "validFrom": "2026-01-01", "validTo": null, "isPrimary": true }
```

Transfer the assignment:
```json
{ "positionId": 8, "effectiveDate": "2026-09-16", "validTo": null }
```

Close any dated resource:
```json
{ "validTo": "2026-09-16" }
```

## Deployment and verification

### Example seed data

`OrganizationStructureSeeder` runs after `OrganizationUnitSeeder` in
`DatabaseSeederService`. It adds missing records only in company `dat`, effective
2026-01-01, using codes to resolve generated IDs:

- Nine units: example company, trading business unit, Western Area, Jeddah Region,
  North Jeddah zone, Showrooms A/B, Finance Department and Central Warehouse.
- Six business roles: Area Manager, Region Manager, Supervisor, Seller,
  Finance Manager and Warehouse Manager.
- `ORG-OPERATIONS`: Company → Business Unit → Area → Region → Zone → Showrooms;
  Warehouse is directly under the Business Unit (eight nodes).
- `ORG-FINANCE`: Company → Business Unit → Finance Department, Showrooms and
  Warehouse (six nodes). These are organizational reporting nodes, not ledger dimensions.
- Seven vacant positions: one for each manager/supervisor responsibility and
  one seller seat in each showroom.

Reruns preserve existing names, parent relationships, dates, disabled records,
and soft-deleted records. Closed node memberships and positions are not recreated.
Units that cannot cover the seed period and unavailable roles/hierarchies are
skipped for dependent seeds. Existing workers and assignments are unchanged;
assign workers explicitly through the management API after reviewing the examples.
The relational seed tests verify repeatability, ancestry, preservation and company scope.

Apply the reviewed EF migration before enabling the APIs/seeder. No live database
is modified as part of scaffolding or tests. The design-time factory supports
`-- --schema-only` for offline EF scaffolding/script generation. Database commands
require an explicit `IXAPI_MIGRATIONS_CONNECTION` environment variable.

SQL Server model tests check mappings and filters; SQLite relational service tests
check company references, transfer rollback, occupancy, primary-assignment overlap,
historical date boundaries, hierarchy purposes and period containment. SQLite tests
do not establish SQL Server concurrency/locking behavior or authenticated live CRUD.

UI screens, transaction-specific fields and module-specific business-policy changes
are separate integrations; this foundation exposes their common backend capability.
