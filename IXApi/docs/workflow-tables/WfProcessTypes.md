# WfProcessTypes

Classifies the overall workflow process type.

- Primary key: `RecId` (`byte`).
- Referenced by: `WfProcesses.ProcessTypeId`.
- Process Builder: required when saving a process.
- Legacy seed guarantees `STD` (Standard), `REV` (Review), and `APP` (Approval).
- This is distinct from `WfActivityTypes`, which classifies individual activities.

## `wf.sql` snapshot

- **Not present** in the legacy SQL export.
- Current process migration requires seeded/default process-type rows.
