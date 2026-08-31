# WfRequestDetails

Historical snapshot of request-control labels and submitted values.

- References a request through `RequestId`; records process, control type and stable request-control ID.
- Stores English/Arabic labels and values, criteria flag, score and order.
- Mail and official print data are resolved from these saved rows, with serialized request XML as a legacy fallback.
- Tracking seed copies current request-control definitions into representative request snapshots.
- New controls added later must not be injected into old request details.

## `wf.sql` snapshot

- **Not present** as a normalized table. Historical request fields are serialized inside `WfRequests.RequestDetails`.
