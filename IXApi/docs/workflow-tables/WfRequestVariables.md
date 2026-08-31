# WfRequestVariables

Runtime variable values associated with a request.

- References `WfRequests` and `WfVariables`.
- Stores `VariableValue` for a submitted request.
- It differs from `WfRequestMappingVariables`, which defines control-to-variable mapping.
- Neither reviewed workflow seeder currently creates these rows.
- Process Builder does not manage this runtime table.

## `wf.sql` snapshot

- Present in the legacy schema with **0 exported rows**.
- Legacy columns are `RequestID`, `VariableID`, and `VariableValue`.
