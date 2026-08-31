# WfAssignments

Runtime assignment of a request to an activity and responsible user.

- References `WfRequests` and `WfActivities`; also stores `StepId`.
- Tracks assigned/finished dates, automatic passing, transfer status and score.
- Drives the Mail tracking timeline and current responsible employee.
- Tracking seed creates an assignment only when a representative request has a valid configured activity.
- Definition changes belong in activity/step tables, not here.

## `wf.sql` snapshot

- Exported rows: **493**.
- Legacy columns cover request, activity, user, assignment date, finished state, auto-passing and hours; the current model adds step, completion date, transfer, score and audit fields.
