# ReportTemplateVersions

Versioned JSON document for a print template.

- Foreign key: `TemplateId -> ReportTemplates`.
- Stores version number, polymorphic `TemplateJson`, publication state, publisher and publication time.
- Published versions are treated as immutable by the application service.
- Tracking seed creates version 1 with supported A4 fields and marks it published.
- `ReportTemplates.CurrentVersionId` identifies the active published version.

## `wf.sql` snapshot

- **Not present**. Legacy workflow data has no equivalent structured JSON template-version table.
