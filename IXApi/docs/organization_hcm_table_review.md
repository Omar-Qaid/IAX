# Organization & HCM Tables Review

## Scope

This document reviews the current HCM and organization-related tables for the following business structure:

### Sales hierarchy

```text
Area Manager
    ↓
Region Manager
    ↓
Supervisor
    ↓
Seller
    ↓
Showroom(s)
```

A showroom can have one or more sellers.

A seller may submit a workflow request:

- For himself/herself
- For a showroom

Managers must be able to see all workers and showrooms below them in the hierarchy.

Example:

```text
Area Manager
└── Region Manager
    ├── Supervisor A
    │   ├── Seller 1
    │   │   └── Showroom A
    │   └── Seller 2
    │       └── Showroom B
    └── Supervisor B
        └── Seller 3
            └── Showroom C
```

### Administrative hierarchy

```text
Boss
    ↓
IT Manager
    ↓
Direct Manager
    ↓
Employee
```

Managers must be able to see all workers below them.

Example:

```text
Boss
└── IT Manager
    ├── Direct Manager A
    │   ├── Employee 1
    │   └── Employee 2
    └── Direct Manager B
        └── Employee 3
```

---

# Recommended Core Model

The recommended core tables are:

```text
HcmWorker
OrganizationUnit
OrganizationRole
OrganizationHierarchy
OrganizationHierarchyNode
HcmWorkerOrganizationAssignment
HcmPosition        -- optional but recommended for enterprise HCM
```

The workflow module should consume this organization model instead of maintaining its own manager structure.

---

# Table Review

## 1. `Nationality`

```csharp
public DbSet<Nationality> Nationalities => Set<Nationality>();
```

### Purpose

Stores employee nationality master data.

Example:

```text
Id | Name
---|----------------
1  | Saudi
2  | Egyptian
3  | Yemeni
4  | Jordanian
```

Normally referenced by:

```text
HcmWorker.NationalityId
```

### Needed for hierarchy/workflow?

**No.**

Nationality does not affect:

- Manager hierarchy
- Showroom assignment
- Workflow visibility
- Reporting relationships

### Recommendation

```text
KEEP only if the system includes HR employee master data.
```

Status:

**Unrelated to organization hierarchy.**

---

## 2. `Occupation`

```csharp
public DbSet<Occupation> Occupations => Set<Occupation>();
```

### Purpose

Stores employee occupations or professions.

Examples:

```text
Software Developer
Accountant
Salesman
Cashier
Warehouse Worker
HR Specialist
```

It usually describes the worker's profession.

### Needed for hierarchy/workflow?

**No.**

Occupation should not determine the reporting hierarchy.

### Recommendation

```text
KEEP only if needed by HR or employee profile features.
```

Status:

**Unrelated to organization hierarchy.**

---

## 3. `Department`

```csharp
public DbSet<Department> Departments => Set<Department>();
```

### Purpose

Stores organizational departments.

Examples:

```text
IT
Finance
Sales
Human Resources
Operations
```

### Problem

You already have:

```text
OrganizationUnit
```

If `OrganizationUnit` supports different organization unit types such as:

```text
Company
Department
Area
Region
Showroom
Branch
Warehouse
```

then a separate `Department` table creates unnecessary duplication.

Example duplication:

```text
Department
-----------
10 | IT

OrganizationUnit
----------------
100 | IT | Department
```

Now the system has two sources for the same organizational concept.

### Needed?

**Usually no, if `OrganizationUnit` supports `Department`.**

### Recommendation

Prefer:

```text
OrganizationUnit
    Type = Department
```

instead of a separate department entity.

Status:

**Potentially redundant.**

---

## 4. `OrganizationUnit`

```csharp
public DbSet<OrganizationUnit> OrganizationUnits => Set<OrganizationUnit>();
```

### Purpose

Represents organizational entities.

This should be one of the most important tables in the organization module.

Recommended unit types:

```text
Company
BusinessUnit
Department
Area
Region
Branch
Showroom
Warehouse
Office
```

Example:

```text
Id   Name                 Type
---  -------------------  ----------
1    Main Company         Company
10   Riyadh               Area
20   Riyadh North         Region
30   Granada Showroom     Showroom
31   Ishbiliyah Showroom  Showroom
40   IT Department        Department
```

### Recommended hierarchy

`OrganizationUnit` may also contain parent-child structure:

```text
Company
└── Riyadh Area
    └── Riyadh North Region
        ├── Granada Showroom
        └── Ishbiliyah Showroom
```

### Needed?

**Yes. Required.**

Use it for:

- Company
- Area
- Region
- Showroom
- Department
- Branch
- Other organization entities

Status:

**Required.**

---

## 5. `OrganizationRole`

```csharp
public DbSet<OrganizationRole> OrganizationRoles => Set<OrganizationRole>();
```

### Purpose

Defines the role a worker performs inside the organization.

Examples:

```text
Seller
Supervisor
Region Manager
Area Manager
Employee
Direct Manager
IT Manager
Boss
Showroom Manager
```

A role should describe responsibility, not necessarily application security.

Do not mix this with system authorization roles such as:

```text
SystemAdmin
AccountOwner
ApplicationAdministrator
```

Those are security roles.

### Example

```text
OrganizationRole
---------------------------
1  Seller
2  Supervisor
3  RegionManager
4  AreaManager
5  DirectManager
6  ITManager
7  Boss
```

### Needed?

**Yes, recommended.**

It helps answer:

```text
What is this worker's organizational responsibility?
```

Status:

**Required/recommended.**

---

## 6. `OrganizationHierarchy`

```csharp
public DbSet<OrganizationHierarchy> OrganizationHierarchies
    => Set<OrganizationHierarchy>();
```

### Purpose

Defines different hierarchy structures.

This is important because your organization has more than one reporting hierarchy.

Example:

```text
Id | Name
---|----------------------------
1  | Sales Hierarchy
2  | Administrative Hierarchy
3  | Approval Hierarchy
```

### Why multiple hierarchies?

A worker may belong to different hierarchy structures.

Example:

```text
Sales Hierarchy

Area Manager
└── Region Manager
    └── Supervisor
        └── Seller
```

and:

```text
Administrative Hierarchy

Boss
└── IT Manager
    └── Direct Manager
        └── Employee
```

These should not be hard-coded into `HcmWorker`.

### Needed?

**Yes. Required.**

Status:

**Required.**

---

## 7. `OrganizationHierarchyNode`

```csharp
public DbSet<OrganizationHierarchyNode> OrganizationHierarchyNodes
    => Set<OrganizationHierarchyNode>();
```

### Purpose

Stores the actual hierarchy tree.

Typical conceptual fields:

```text
Id
HierarchyId
WorkerId / PositionId / OrganizationUnitId
ParentNodeId
SortOrder
ValidFrom
ValidTo
```

Example:

```text
Hierarchy = Sales

Area Manager
    ↓
Region Manager
    ↓
Supervisor
    ↓
Seller
```

Possible data:

```text
NodeId | HierarchyId | WorkerId | ParentNodeId
------ | ----------- | -------- | ------------
1      | 1           | 100      | NULL
2      | 1           | 110      | 1
3      | 1           | 120      | 2
4      | 1           | 130      | 3
5      | 1           | 131      | 3
```

This allows recursive queries such as:

```text
Get all descendants of Region Manager
```

and then:

```text
Supervisor
Seller 1
Seller 2
...
```

### Needed?

**Yes. Required.**

This should be the main source for hierarchical visibility.

Status:

**Required.**

---

## 8. `HcmWorkerManagementLevel`

```csharp
public DbSet<HcmWorkerManagementLevel> HcmWorkerManagementLevels
    => Set<HcmWorkerManagementLevel>();
```

### Possible purpose

Usually represents management levels such as:

```text
Level 1 - Employee
Level 2 - Supervisor
Level 3 - Manager
Level 4 - Regional Manager
Level 5 - Area Manager
```

### Problem

For your use case, the actual reporting relationship is already represented by:

```text
OrganizationHierarchy
OrganizationHierarchyNode
OrganizationRole
```

A management level does not tell us who manages whom.

Example:

```text
Ali = Level 3
Mohammed = Level 2
```

This still does not tell the system:

```text
Ali manages Mohammed
```

### Needed?

**No for hierarchy traversal.**

It may still be useful for HR classification, salary bands, approval rules, or reporting.

### Recommendation

If it is only being used to reproduce:

```text
Seller -> Supervisor -> Region Manager -> Area Manager
```

then remove it from the hierarchy design.

Status:

**Not needed for this feature.**

---

## 9. `HcmWorkerManager`

```csharp
public DbSet<HcmWorkerManager> HcmWorkerManagers
    => Set<HcmWorkerManager>();
```

### Possible purpose

Stores direct relationships such as:

```text
WorkerId
ManagerWorkerId
```

Example:

```text
Seller 1     -> Supervisor A
Supervisor A -> Region Manager A
```

### Problem

This duplicates the same relationship stored in:

```text
OrganizationHierarchyNode
```

Using both creates two sources of truth.

Example:

```text
HcmWorkerManager

Seller 1 -> Supervisor A
```

but:

```text
OrganizationHierarchyNode

Seller 1 -> Supervisor B
```

Now the application does not know which manager is correct.

### Needed?

**No, if `OrganizationHierarchyNode` is the authoritative reporting structure.**

### Recommendation

Use:

```text
OrganizationHierarchy
+
OrganizationHierarchyNode
```

for reporting relationships.

Do not maintain the same relationship in `HcmWorkerManager`.

Status:

**Redundant for the proposed architecture.**

---

## 10. `HcmWorkerOrganizationAssignment`

```csharp
public DbSet<HcmWorkerOrganizationAssignment>
    HcmWorkerOrganizationAssignments
    => Set<HcmWorkerOrganizationAssignment>();
```

### Purpose

Connects workers to organizational units.

This table is critical for the showroom requirement.

Example:

```text
Worker     OrganizationUnit   Role
---------  -----------------  --------
Seller 1   Showroom A         Seller
Seller 2   Showroom A         Seller
Seller 3   Showroom B         Seller
```

Therefore:

```text
Showroom A
├── Seller 1
└── Seller 2
```

### Recommended fields

```text
Id
WorkerId
OrganizationUnitId
OrganizationRoleId
ValidFrom
ValidTo
IsPrimary
```

### History

Suppose Seller 1 moves from Showroom A to Showroom B.

Do not delete/update the old relation.

Keep:

```text
Seller 1 -> Showroom A
ValidFrom = 2025-01-01
ValidTo   = 2026-08-31
```

and insert:

```text
Seller 1 -> Showroom B
ValidFrom = 2026-09-01
ValidTo   = NULL
```

This preserves historical workflow requests correctly.

### Needed?

**Yes. Required.**

Status:

**Required.**

---

## 11. `HcmWorker`

```csharp
public DbSet<HcmWorker> HcmWorkers => Set<HcmWorker>();
```

### Purpose

Represents the employee/worker.

All of the following are workers:

```text
Employee
Seller
Supervisor
Region Manager
Area Manager
Direct Manager
IT Manager
Boss
```

Do not create different employee tables for:

```text
Seller
Manager
Supervisor
```

They are all workers with different:

```text
Positions
Roles
Assignments
Hierarchy memberships
```

### Example

```text
WorkerId | Name
-------- | ----------------
100      | Ahmed
101      | Ali
102      | Mohammed
```

Then organization data determines their responsibilities.

### Needed?

**Yes. Core table.**

Status:

**Required.**

---

## 12. `HcmPosition`

```csharp
public DbSet<HcmPosition> HcmPositions => Set<HcmPosition>();
```

### Purpose

Represents a position/job slot in the organization.

Examples:

```text
Salesman - Granada Showroom
Supervisor - Riyadh North
Region Manager - Central Region
IT Manager
Finance Manager
```

A worker occupies a position.

Enterprise HCM systems normally separate:

```text
Worker
```

from:

```text
Position
```

because workers can move while positions continue to exist.

Example:

```text
Position:
IT Manager

2025:
Ahmed occupies IT Manager

2026:
Ahmed leaves
Ali occupies IT Manager
```

The hierarchy can remain attached to the position even if the worker changes.

### Needed?

For a small/simple system:

```text
Optional
```

For an enterprise HCM/workflow system:

```text
Recommended
```

### Recommendation

Keep it if you want:

- Position history
- Vacant positions
- Replacing workers without rebuilding hierarchy
- Enterprise-grade organization management
- Position-based workflow assignment

Status:

**Optional but recommended.**

---

## 13. `HcmWorkerGroup`

```csharp
public DbSet<HcmWorkerGroup> HcmWorkerGroups
    => Set<HcmWorkerGroup>();
```

### Purpose

Normally represents arbitrary collections of workers.

Examples:

```text
Inventory Team
Audit Team
Sales Team A
Emergency Response Team
Approval Group
```

Groups are different from hierarchy.

A group could contain:

```text
Employee A
Employee B
Employee C
```

without any parent-child relationship.

### Needed for your hierarchy?

**No.**

Your seller/supervisor/manager relationship should come from hierarchy, not groups.

### Keep only if

You need arbitrary teams or workflow assignment groups.

Status:

**Not needed for the current requirement.**

---

## 14. `HcmWorkerGroupDetail`

```csharp
public DbSet<HcmWorkerGroupDetail> HcmWorkerGroupDetails
    => Set<HcmWorkerGroupDetail>();
```

### Purpose

Contains members of `HcmWorkerGroup`.

Example:

```text
GroupId | WorkerId
------- | --------
1       | 100
1       | 101
1       | 105
```

### Needed?

If you remove/do not use:

```text
HcmWorkerGroup
```

then this table is also unnecessary.

Status:

**Not needed unless Worker Groups are required.**

---

## 15. `HcmWorkerCategory`

```csharp
public DbSet<HcmWorkerCategory> HcmWorkerCategories
    => Set<HcmWorkerCategory>();
```

### Possible purpose

Classifies workers.

Examples:

```text
Permanent
Temporary
Contractor
Part Time
Sales
Administration
Field Worker
Office Worker
```

### Needed for hierarchy?

**No.**

Category does not define:

```text
Who reports to whom
```

or:

```text
Which showroom belongs to which seller
```

### Recommendation

Keep only if employee categorization is a real HR requirement.

Status:

**Not needed for this hierarchy/workflow requirement.**

---

## 16. `HcmWorkerCategoryGroup`

```csharp
public DbSet<HcmWorkerCategoryGroup> EmployeeCategoryGroups
    => Set<HcmWorkerCategoryGroup>();
```

### Possible purpose

Groups worker categories.

Example:

```text
Employee Type
├── Permanent
├── Temporary
└── Contractor
```

or:

```text
Business Classification
├── Sales
├── Administration
└── Operations
```

### Needed?

**No for current organization hierarchy.**

Keep only if category grouping is used elsewhere in HCM.

Status:

**Not needed for this feature.**

---

# Summary

## Required

These tables should form the core organization model:

```text
HcmWorker
OrganizationUnit
OrganizationRole
OrganizationHierarchy
OrganizationHierarchyNode
HcmWorkerOrganizationAssignment
```

---

## Optional / Recommended

```text
HcmPosition
```

Recommended for enterprise systems because positions survive worker changes and provide cleaner history.

---

## Not Needed for the Current Requirement

```text
HcmWorkerManagementLevel
HcmWorkerManager
HcmWorkerGroup
HcmWorkerGroupDetail
HcmWorkerCategory
HcmWorkerCategoryGroup
```

These may be useful for other HCM features, but they are not necessary to implement the hierarchy and workflow visibility described here.

---

## Unrelated HR Master Data

```text
Nationality
Occupation
```

Keep them only if employee profile/HR functionality requires them.

---

## Potentially Redundant

```text
Department
```

If `OrganizationUnit` already supports:

```text
Type = Department
```

then use `OrganizationUnit` instead and avoid a separate `Department` table.

---

# Final Recommended Structure

```text
HcmWorker
│
├── HcmPosition                         (optional/recommended)
│
├── OrganizationHierarchyNode
│       │
│       └── OrganizationHierarchy
│              ├── Sales Hierarchy
│              └── Administrative Hierarchy
│
└── HcmWorkerOrganizationAssignment
        │
        ├── OrganizationRole
        │      ├── Seller
        │      ├── Supervisor
        │      ├── Region Manager
        │      ├── Area Manager
        │      ├── Direct Manager
        │      ├── IT Manager
        │      └── Boss
        │
        └── OrganizationUnit
               ├── Company
               ├── Department
               ├── Area
               ├── Region
               ├── Showroom
               └── Branch
```

---

# Workflow Access Concept

The workflow should not store its own manager chain.

Instead:

```text
Current User
    ↓
Find current worker
    ↓
Find hierarchy node
    ↓
Get all descendant workers
    ↓
Get OrganizationUnit assignments for those workers
    ↓
Filter workflow requests
```

Example:

```text
Region Manager
    ↓
Supervisor A
    ↓
Seller 1
Seller 2
```

The Region Manager can see:

```text
Supervisor A requests
Seller 1 requests
Seller 2 requests
Showroom requests related to Seller 1
Showroom requests related to Seller 2
```

The same generic logic works for:

```text
Boss
    ↓
IT Manager
    ↓
Direct Manager
    ↓
Employee
```

No sales-specific hierarchy logic needs to exist inside the workflow module.

---

# Recommended Removal Candidates

If these entities are currently used only for the hierarchy described in this document, they can be removed or excluded from the organization module:

```csharp
public DbSet<HcmWorkerManagementLevel> HcmWorkerManagementLevels
    => Set<HcmWorkerManagementLevel>();

public DbSet<HcmWorkerManager> HcmWorkerManagers
    => Set<HcmWorkerManager>();

public DbSet<HcmWorkerGroup> HcmWorkerGroups
    => Set<HcmWorkerGroup>();

public DbSet<HcmWorkerGroupDetail> HcmWorkerGroupDetails
    => Set<HcmWorkerGroupDetail>();

public DbSet<HcmWorkerCategory> HcmWorkerCategories
    => Set<HcmWorkerCategory>();

public DbSet<HcmWorkerCategoryGroup> EmployeeCategoryGroups
    => Set<HcmWorkerCategoryGroup>();
```

Potentially also:

```csharp
public DbSet<Department> Departments => Set<Department>();
```

if departments are represented by:

```text
OrganizationUnit.Type = Department
```

Do not delete tables solely because they are not needed by workflow. First verify whether other HR or application modules use them.
