# WfUsersPerformers

Associates performer definitions with user/employee identifiers.

- Entity name: `WfPerformerUsers`; SQL table: `WfUsersPerformers`.
- Foreign key: `PerformerId -> WfPerformers`.
- Stores `UserID`, optional related field and extended properties.
- Used to resolve concrete assignees.
- Legacy seed inserts rows only for existing performers.

## `wf.sql` snapshot

- Exported rows: **25**.
- Legacy rows connect performer IDs to user IDs with optional related-field and extended metadata.
