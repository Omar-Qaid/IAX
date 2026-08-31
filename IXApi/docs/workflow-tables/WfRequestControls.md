# WfRequestControls

Controls placed on the form used to submit a new workflow request.

- References `WfProcesses` and the control type in `WfControls`.
- Stores label/code, order, score, legacy `ValidationRules`, and JSON/XML-like `ExtendedProperties`.
- Process Builder directly manages these rows.
- Options and normalized validations are child tables.
- Legacy seed imports the original process request-form definitions.
- Submitted values are copied to `WfRequestDetails`; definitions must not be used as historical request data.

## `wf.sql` snapshot

- Exported rows: **35**.
- Legacy rows contain bilingual labels, lookup table/member metadata, layout coordinates, related object, mandatory/criteria flags and default values.
- The current model consolidates legacy UI metadata into `ExtendedProperties` and normalized child tables.
