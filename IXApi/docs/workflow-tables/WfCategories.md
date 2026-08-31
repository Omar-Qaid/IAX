# WfCategories

Process-category master used to group workflows in navigation and request-submission screens.

- Primary key: `RecId` (`short`).
- Referenced by: `WfProcesses.CategoryId`.
- Process Builder: required when saving a process.
- Legacy seed: imports category rows from `LegacyWorkflowMasterData.json`; legacy ID `0` is intentionally excluded as a sentinel.
- Typical data: Human Resources, Sales, IT, Finance, Purchasing and other organizational workflow groups.

## `wf.sql` snapshot

- Exported rows: **12**.
- Legacy schema stores bilingual name/description, activation, ordering and system-category flags.
- The current importer maps those values into common code/name/description and standard active/system fields.
