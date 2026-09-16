# Shared Organization implementation

## Objective

Organization owns company-scoped units, multiple hierarchies, business roles,
positions and effective-dated worker assignments. Finance, Sales, Inventory,
Administration, HR and Workflow consume the same contracts in Shared. Business
roles do not grant security permissions. Existing module transactions and their
business rules remain authoritative.

## Delivery checkpoints

1. Define shared contracts and compatibility rules.
2. Implement entity mappings, company boundaries and a schema migration.
3. Implement management APIs and reusable dated queries, with validation.
4. Verify model, history, overlap, hierarchy and authorization behavior; document
   integration examples and deployment limits.

This delivery builds the backend capability for every module. It does not invent
organization fields on existing Finance/Inventory transactions or change approval
behavior. Those integrations and management screens require their own business
requirements and are follow-on work.

## Decisions

- Keep HcmWorker and its existing identifiers; do not create another Employee table.
- Keep OrganizationUnit and HcmWorkerOrganizationAssignment IDs and existing
  workflow references. Add nullable PositionId for compatibility; new APIs require it.
- Preserve legacy AssignmentRole values and direct ParentOrganizationUnitId. New
  hierarchies use OrganizationHierarchyNode exclusively. No automatic legacy-parent
  fallback or silent synchronization between independently configured hierarchies.
- The recorded migration baseline has no unit/assignment tables: the migration
  creates them and preserves the existing uncommitted optional request fields.
  The existing example seeder defaults to company `dat`. Manually provisioned
  databases need schema/ownership reconciliation before applying this migration.
  New API records use the authorized execution company.
- Use configurable business role codes, separate from Identity roles. Each position
  is a seat in one unit with one business role; multiple sellers use multiple seats.
- Dates use [ValidFrom, ValidTo): an end date is exclusive. Transfer on September 16
  closes the old assignment on September 16 and starts the new one that day.
- Close records rather than deleting or moving historical assignments. Position
  unit/role and hierarchy-node parent are immutable. Reorganization creates dated
  replacements; parent intervals must contain child intervals.
- A worker has at most one primary assignment at a time per company; a position
  has at most one occupant at a time. Serializable writes protect overlap checks.
- Multiple hierarchy purposes coexist. Consumers explicitly choose hierarchy and
  effective date. A vacant role returns no workers; ambiguity returns all eligible
  occupants, leaving selection/escalation to the consuming module.
- Query contracts live in Shared and contain DTOs, not EF entities. Organization
  implements and registers them. Every module can inject the same interface.
- Management APIs use Organization.Structure permissions. Services additionally
  validate execution-company authorization and referenced-record ownership.

## Acceptance

- Model and migration include all foundation tables and preserved optional request links.
- Same codes can exist in different companies; cross-company relationships are rejected.
- Invalid dates, overlap, repeating units in ancestry and incompatible parent periods
  are rejected. Historical queries return the assignment valid on the supplied date.
- Tests exercise rules and EF metadata; compilation covers the host and module graph.
- Migration is generated/reviewed, not applied automatically to a live database.

## Progress

- [x] Shared query contracts, company scope and compatibility decisions.
- [x] Six-table persistence foundation and generated migration/snapshot:
  `20260916111733_SharedOrganizationFoundation`.
- [x] Authorized management endpoints and shared directory registration.
- [x] Transfer/closure, period containment, hierarchy ancestry, role occupants and
  overlap validation with serializable writes and rollback cleanup.
- [x] Focused verification: 22 passed, 0 failed, 0 skipped. Includes eight SQLite
  relational service tests, two SQL Server model tests and twelve existing company
  isolation tests. Snapshot comparison reports no pending model changes.
- [x] Organization module Release build: 1 existing Administration warning, 0 errors.
  Host/test graph build: 168 warnings, 0 errors; final incremental build: 0 warnings,
  0 errors. No warnings originated in the new Structure implementation.
- [x] Offline SQL script generated and reviewed: six CREATE TABLE operations,
  four nullable request-context columns, indexes and foreign keys; no DROP/DELETE
  operations in the forward script.

## Remaining deployment/integration work

- Migration has not been applied to a live database. SQL Server locking under
  concurrent requests, authenticated HTTP CRUD and production seed execution have
  not been exercised. Existing startup initialization may apply pending migrations;
  review the script before starting the API against a live database.
- Configure hierarchy purposes, positions and assignments through the new APIs.
  `OrganizationStructureSeeder` now provides example company `dat` data: nine units,
  six roles, two hierarchies (14 nodes), and seven vacant positions. No real staff
  assignments are inferred; reruns preserve configured records.
- Management UI and module-specific transaction integration remain separate work.
  Shared contracts and examples are ready for Finance, Sales, Inventory, HR,
  Administration and Workflow; no consuming transaction behavior was changed.
