# WfProcessData

Runtime processing record associated with an assignment.

- Optional foreign key: `AssignmentID -> WfAssignments`.
- Stores finish date, serialized activity-detail XML and extended properties.
- Acts as the parent execution record whose ID is placed in `WfActivityDetails.ProcessId`.
- Tracking seed creates an empty record for representative open executions and detailed rows for legacy request `94037`.
- Despite its name, this is execution data rather than the process definition header.

## `wf.sql` snapshot

- Exported rows: **315**.
- Legacy rows associate an assignment with finish date and serialized activity details.
- `WorkflowRequestTrackingSeeder` uses the same XML snapshot pattern for imported and representative execution records.
