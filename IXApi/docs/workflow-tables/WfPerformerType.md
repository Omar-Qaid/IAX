# WfPerformerType

Master classification for performer resolution.

- Primary key: `RecId` (`short`).
- Referenced by `WfPerformers.PerformerTypeId`.
- Legacy seed guarantees a `RELATIONAL` type.
- It describes the resolution strategy; concrete users are linked through `WfUsersPerformers`.

## `wf.sql` snapshot

- **Not present** as a standalone table in the legacy export.
- `LegacyWorkflowMasterDataSeeder` supplies `RELATIONAL` for imported performers.
