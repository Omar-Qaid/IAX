# WfDataTypes

Master catalog for workflow variable data types.

- Primary key: `RecId` (`byte`).
- Referenced by: `WfVariables.DataTypeId`.
- Process Builder maps these records to text, number, boolean, date, and object-like variable behavior.
- Legacy seed imports integer, string, date/time and true/false types.
- Variable validation and transition comparison depend on this type.

## `wf.sql` snapshot

- Exported rows: **4** for integer, string, date/time and true/false concepts.
- Legacy schema carries bilingual names/descriptions; current frontend normalizes them to its builder data types.
