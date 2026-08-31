# WfSteps

Ordered stages within a workflow process.

- Primary key: `RecId` (`long`).
- Foreign key: `ProcessId -> WfProcesses`.
- Owns: `WfActivities`; transitions target a step through `WfTransitions.StepId`.
- Stores order, score, automatic-passing hours, mandatory/system flags and active state.
- Process Builder directly creates, updates, orders and removes these rows.
- Legacy seed imports steps after their parent processes.

## `wf.sql` snapshot

- Exported rows: **47**.
- Legacy columns cover bilingual labels/descriptions, process, order, active, auto-passing, mandatory, score and system flags.
- These steps parent the 52 exported activities.
