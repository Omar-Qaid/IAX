# ReportEntityVersions

Associates any referenced business record with a selected report-template version.

- Uses `RefTableId` and `RefRecId`, following the `DocuRef` polymorphic-reference concept.
- References `ReportTemplates` and `ReportTemplateVersions`; the owning module validates the referenced business record.
- Stores selection timestamp and selecting user.
- Intended to pin historical official output to a stable template version.
- Workflow seeds use the `WfRequests` table ID and create or repair the association for each representative request/template pair.
- Current `GetPublishedForRequestAsync` still resolves the template's current published version rather than consulting this association; full historical pinning is therefore not yet enforced by that endpoint.

## `wf.sql` snapshot

- **Not present**. Historical request-to-template version pinning is a current-project addition.
