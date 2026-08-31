# WfRequestMappingVariables

Maps a request control into a workflow variable.

- References `WfRequestControls` and `WfVariables`.
- `SortOrder` controls mapping order.
- Legacy seed imports rows only when both references exist.
- Current `processBuilderApi.ts` does not load or save this table.
- This is definition data, not a stored variable value.

## `wf.sql` snapshot

- Exported rows: **4**.
- Legacy columns map `RequestControlID` to `VariableID` with an active flag.
- Current seed import validates both foreign keys before inserting a mapping.
