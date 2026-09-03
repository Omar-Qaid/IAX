# ReportTemplates

Generic record-owned header for a reusable report or print template.

- Uses the same polymorphic record-reference concept as `DocuRef`: `RefTableId` identifies the entity type and `RefRecId` identifies its record.
- Stores code/name, page size, orientation, language, default flag, lifecycle status and `CurrentVersionId`.
- The document body is not stored here; it belongs in version rows.
- Workflow integration uses the `WfProcesses` table ID while other modules can use their own registered table IDs.
- Unique company-scoped indexes apply to `(DataAreaId, RefTableId, RefRecId, Code)` and one filtered default per referenced record.

## `wf.sql` snapshot

- **Not present**. The structured print-template engine is a current-project addition.
