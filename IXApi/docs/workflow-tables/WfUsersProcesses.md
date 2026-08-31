# WfUsersProcesses

Scopes a process to employees, occupations or departments.

- Foreign key: `ProcessId -> WfProcesses`.
- Optional references: `EmployeeId`, `OccupationId`, `DepartmentId`.
- Used by process eligibility and representative-assignment logic.
- Legacy seed validates optional organization references before inserting.
- Current Process Builder does not fully expose all three targeting modes.

## `wf.sql` snapshot

- Exported rows: **38**.
- Legacy rows target a process by optional department, occupation or employee.
- Current importer validates each organization reference before inserting.
