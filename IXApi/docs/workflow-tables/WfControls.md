# WfControls

Master catalog of UI control types available to request and activity forms.

- Primary key: `RecId` (`byte`).
- Referenced by: `WfRequestControls.ControlId` and `WfActivityControls.ControlId`.
- Fields such as `Code`, `Name`, and `ControlType` are normalized by the frontend into control behaviors.
- Legacy seed imports number, text, textarea, date, dropdowns, checkbox, table, label, radio, employee, file, signature, location and related control types.
- This table defines a type; it does not store a control placed on a particular process.

## `wf.sql` snapshot

- Exported rows: **15**.
- Legacy columns provide control ID, English/Arabic names and descriptions, and a textual `ControlType`.
- The exported set is smaller than the current imported catalog, so control coverage must be compared by stable ID before migration.
