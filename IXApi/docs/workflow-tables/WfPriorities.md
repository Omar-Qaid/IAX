# WfPriorities

Priority lookup for workflow process definitions.

- Primary key: `RecId` (`byte`).
- Referenced by: `WfProcesses.PriorityId`.
- Process Builder: required when saving a process.
- Legacy seed guarantees `LOW`, `MED`, and `HIGH` rows before importing processes.
- It classifies the process; runtime urgency behavior must be implemented separately if required.

## `wf.sql` snapshot

- **Not present** in the legacy SQL export.
- Current process migration therefore requires seeded/default priority rows.
