# WfPerformers

Defines who or what can perform a workflow activity.

- Referenced by `WfActivities.PerformerId`.
- References `WfPerformerType`.
- Supports applicant/employee and manager-chain flags plus relational metadata.
- Process Builder requires a performer for each activity.
- Legacy seed imports performers before dependent activities.

## `wf.sql` snapshot

- Exported rows: **39**.
- Legacy schema stores employee/manager-level flags, related field and SQL table/field/where metadata directly on performers.
- The current importer maps these rows to performer type `RELATIONAL`.
