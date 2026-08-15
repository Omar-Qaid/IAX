# List-details pattern

`ListDetailsPage` supports two contracts.

## Standard variant

The legacy/default variant accepts `title`, optional subtitle/action pane, `DataGridProps`, optional detail pane, loading, selection, and dialogs.

## Enterprise variant

`variant="enterprise"` accepts `EnterpriseListDetailsConfig<T>`. It supports static, controlled, or remote repositories; list or grid master presentation; search/filter and related-information panels; CRUD permissions; synchronous/async validation; typed detail sections/fields; number-sequence metadata; optional pane resizing/persistence; and confirmation around destructive or dirty transitions.

Module examples include currency, legal entity, number sequence, workflow process/step/activity, payment mode, and payment term pages. Define mapping functions (`getValues`/`setValues`) and `createRecord` at the module boundary. Remote repositories receive an `AbortSignal` for loads and explicit create/update/delete functions.

Use stable record IDs and do not mix a second uncontrolled form state with the pattern draft.
