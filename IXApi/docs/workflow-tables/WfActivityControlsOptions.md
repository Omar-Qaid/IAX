# WfActivityControlsOptions

Selectable entries or table-column definitions for an activity control.

- Foreign key: `ActivityControlId -> WfActivityControls`.
- Used for manual dropdown, radio-button, checkbox-list and table activity controls.
- Stores value, display name and sort order.
- Process Builder directly synchronizes the table.
- These are definition rows, not the values selected by a user during execution.
- Neither reviewed workflow seeder currently creates normalized activity-option rows.

## `wf.sql` snapshot

- **Not present** in the legacy export; legacy options are embedded in activity-control metadata.
