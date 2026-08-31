# WfRequests

Runtime header for a submitted workflow request.

- References `WfProcesses` and optionally an employee.
- Stores request date, serialized detail XML, status dates, score, progress and notes.
- Submission writes one request plus historical rows in `WfRequestDetails`.
- Tracking seed preserves request `94037` and ensures at least one representative request per active process.
- This is transaction data and must not be changed when a process definition is edited.

## `wf.sql` snapshot

- Exported rows: **203**.
- Legacy rows contain employee, process, date, serialized request details, finished/stopped dates, score and progress.
- Current request/detail migration separates normalized `WfRequestDetails` while retaining serialized XML compatibility.
