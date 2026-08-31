# WfActivityControlsValidations

Normalized validation rules for activity controls.

- Foreign key: `ActivityControlId -> WfActivityControls`.
- Stores type, expression, operator, value, mask, message, severity and order.
- Process Builder directly synchronizes these rows.
- Validation targets `ActivityControlId`, not the parent `ActivityId`.
- Legacy mandatory flags are primarily imported into `WfActivityControls.ValidationRules`.
- Neither reviewed seeder converts those legacy flags into normalized rows here.

## `wf.sql` snapshot

- **Not present** in the legacy export; mandatory behavior is stored on `WfActivityControls`.
