# WfRequestControlsValidations

Normalized validation rules for request-form controls.

- Foreign key: `RequestControlId -> WfRequestControls`.
- Stores validation type, expression, operator, value, mask, message, severity, order and active state.
- Read by `WfRequestService` and `ValidationEngine` during definition loading and submission.
- Process Builder directly synchronizes this table.
- Legacy seed primarily writes old mandatory XML to `WfRequestControls.ValidationRules`, not normalized rows here.

## `wf.sql` snapshot

- **Not present** in the legacy export; required flags are stored directly on `WfRequestControls`.
