# WfActivities

Executable activity definitions placed inside workflow steps.

- Primary key: `RecId` (`long`).
- Required references: `StepId`, `ActivityTypeId`, and `PerformerId`.
- Owns: activity controls; referenced by assignments and optional transition triggers.
- Stores score, notification flags, previous-step/document visibility, mandatory-document and auto-pass settings.
- Process Builder directly creates and updates activities.
- Legacy seed imports activities with their step, performer and execution settings.
- Current review note: backend `WfActivity`/DTO has no persisted `SortOrder`, although the frontend expects one.

## `wf.sql` snapshot

- Exported rows: **52**. Legacy IDs begin around `6033`; examples include Area Supervisor, Showroom Manager, HR Officer and Finance Officer activities.
- Legacy columns also contain bilingual names/descriptions, print flags, stop flags, alerts and auto-passing hours. Several of those columns are consolidated, renamed or absent in the current entity.
