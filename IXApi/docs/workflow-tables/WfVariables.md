# WfVariables

Named variables belonging to a process definition.

- Primary key: `RecId`; references `WfProcesses` and `WfDataTypes`.
- Used by transitions and request/activity control mappings.
- Process Builder directly saves code, name, type, order and active state.
- Legacy seed imports variables before mappings and transitions.
- Runtime values belong in `WfRequestVariables` or `WfProcessVariables`.

## `wf.sql` snapshot

- Exported rows: **21**.
- Rows store process, data type, bilingual name/description, order and activation.
- Their IDs are used by 17 activity mappings, 4 request mappings and 30 transitions.
