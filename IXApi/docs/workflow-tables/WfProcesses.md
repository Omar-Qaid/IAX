# WfProcesses

Root definition table for a workflow.

- Primary key: `RecId` (`long`).
- Required references: `CategoryId`, `PriorityId`, and `ProcessTypeId`.
- Owns: steps, variables, request controls, transitions, user scope, requests and print templates.
- Process Builder stores name, description, score, repeat/doc flags and active state here.
- Legacy seed imports all process headers before dependent definitions.
- Tracking seed reads active processes and ensures representative requests and print templates without replacing existing rows.

## `wf.sql` snapshot

- Exported rows: **7**.
- Legacy columns cover bilingual name/description, category, active, repeat/docs, score and system flags.
- Current `PriorityId` and `ProcessTypeId` do not exist in the legacy table and require migration defaults.
