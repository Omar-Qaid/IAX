# WfActivityControls

Controls displayed while a workflow activity is being processed.

- References `WfActivities`; also stores `ProcessId` and a `ControlId` from `WfControls`.
- Stores label/code, order, score, validation metadata and extended UI properties.
- Process Builder directly creates and updates these rows under each activity.
- Options and validations are stored in child tables.
- Legacy seed imports the activity-form controls from its snapshot.
- Completed values are copied to `WfActivityDetails`.

## `wf.sql` snapshot

- Exported rows: **108**.
- Legacy rows store bilingual labels, row/column positions, lookup members, default value, related object and mandatory/search flags directly on this table.
- Current Process Builder moves selectable entries and normalized validations into child tables while retaining compatibility metadata in `ExtendedProperties`.
