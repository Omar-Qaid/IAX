# WfActivityDetails

Historical values captured when an activity form is processed.

- References an assignment through `AssignmentID`; `ProcessId` points to the related `WfProcessData` execution record in current seed usage.
- Stores control type, stable activity-control ID, bilingual labels/values and order.
- Mail history uses these rows for activity notes while excluding signature/serialized values.
- Tracking seed imports detailed legacy activity values and creates a placeholder for representative executions.
- These rows must remain historical when activity definitions change.

## `wf.sql` snapshot

- **Not present** as a normalized table. Legacy activity details are serialized in `WfProcessData.ActivityDetails`.
