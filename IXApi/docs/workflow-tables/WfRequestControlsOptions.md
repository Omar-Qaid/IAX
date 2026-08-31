# WfRequestControlsOptions

Selectable entries or table-column definitions for a request control.

- Foreign key: `RequestControlId -> WfRequestControls`.
- Used by manual dropdown, radio-button, checkbox-list and table controls.
- Stores value, display name, score, order and option feature JSON.
- Process Builder directly synchronizes these rows.
- Not populated by the current legacy master-data import because the legacy snapshot carries older embedded properties instead.
- `WorkflowRequestTrackingSeeder` reads request-control definitions but does not create option-definition rows.

## `wf.sql` snapshot

- **Not present** in the legacy export; selectable metadata is embedded in request-control columns/properties.
