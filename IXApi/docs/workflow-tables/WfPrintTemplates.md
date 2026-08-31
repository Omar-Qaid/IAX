# WfPrintTemplates

Process-owned header for an official print template.

- Foreign key: `ProcessId -> WfProcesses`.
- Stores code/name, page size, orientation, language, default flag, lifecycle status and `CurrentVersionId`.
- The document body is not stored here; it belongs in version rows.
- Tracking seed creates a published default A4 template only when the process-specific seed template is missing.
- The print-template service manages draft, publish and archive behavior.

## `wf.sql` snapshot

- **Not present**. The structured print-template engine is a current-project addition.
