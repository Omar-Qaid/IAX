# Forms and validation

## Layout and shells

- `FormRow` is a MUI grid container with configurable spacing.
- `FormColumn` defaults to `xs=12`, `sm=6`, `md=4`, `lg=3`.
- `FormContainer`, `FormSection`, `FormFieldGroup`, and `FormActions` provide neutral layout surfaces.
- `EntityForm` is a native form shell with error summary and optional submit/cancel/actions.
- `FormValidationSummary` works with React Hook Form errors; `FormErrorSummary` renders a generic error collection.

## State options

Use React Hook Form with Zod when a feature already uses schema-driven forms, such as application settings. Use `useEntityForm` for a small typed local form with a synchronous validator and submit callback. Enterprise list/details patterns and DataGrid have their own draft/edit lifecycle; do not add a second form store around them without a clear need.

Shared Zod helpers live in `shared/validation`: required text, email, URL, number/date coercion, reusable messages, and Zod-issue mapping. Domain schemas stay with the module.

## Validation flow

Validate before mutation, map correctable field errors to inputs or a summary, and keep server `ApiError.validationErrors` available for display. Disable repeated submissions during pending work. Dirty-state protection is explicit; add `useUnsavedChanges` or the guard component when the form can lose user changes.
