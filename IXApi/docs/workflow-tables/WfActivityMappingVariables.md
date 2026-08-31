# WfActivityMappingVariables

Maps an activity control into a workflow variable.

- References `WfActivityControls` and `WfVariables`.
- `VariableOrder` controls mapping order.
- Legacy seed filters out mappings with missing controls or variables.
- Current Process Builder frontend does not load or save this table.
- This table is separate from activity-triggered transition routing.

## `wf.sql` snapshot

- Exported rows: **17**.
- Legacy columns are `MappingId`, `ActivityControlID`, `VariableID`, and `Activated`.
- The current entity replaces the legacy active flag with standard entity state and exposes `VariableOrder`.
