# WfProcessVariables

Runtime variable-value table whose current entity also references a request.

- Fields: `RequestId`, `VariableId`, `VariableValue`, `SortOrder`.
- Despite the name, the model has no `ProcessId`; it behaves as request-scoped data.
- Neither reviewed workflow seeder currently populates it.
- Clarify this naming/model mismatch before adding new dependencies.
- Process variable definitions belong in `WfVariables`.

## `wf.sql` snapshot

- Exported rows: **1,141**, the largest variable-value set in this export.
- Legacy columns are request, variable, value and order, confirming that the table is request-scoped despite its name.
