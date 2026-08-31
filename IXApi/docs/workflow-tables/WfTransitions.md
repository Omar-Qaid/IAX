# WfTransitions

Conditional routing rules for a process.

- References process, variable, operator and target step.
- Optional trigger is `ActivityId` or `RequestControlId`.
- Stores comparison value, sort order and active state.
- Process Builder directly loads and saves this table.
- Legacy seed inserts only rows whose referenced definition records exist.

## `wf.sql` snapshot

- Exported rows: **30**.
- Rows reference process, optional activity/request control, variable, operator, value and target step.
- The importer filters orphaned references.
