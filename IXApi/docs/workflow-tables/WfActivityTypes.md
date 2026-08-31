# WfActivityTypes

Master classification for individual workflow activities.

- Primary key: `RecId` (`byte`).
- Referenced by: `WfActivities.ActivityTypeId`.
- Process Builder selects a backend activity type for approval, review, notification, data-entry or API-oriented designer modes.
- Legacy seed imports the distinct activity types from the legacy snapshot.
- It is not the same as `WfProcessTypes`, which classifies the complete process.

## `wf.sql` snapshot

- Exported rows: **2**, using legacy IDs `0` and `1` for normal and partial stages.
- The current legacy importer excludes zero-key sentinel rows, so ID `0` requires explicit remapping when migrating this table.
