# Dynamic print template engine

## 1. Existing architecture discovered

- The backend is a .NET 9 modular monolith. Workflow owns processes, request definitions, submitted request data, steps, activities, assignments, and execution history. `ApplicationDbContext` is the shared EF Core SQL Server context and applies module-owned entity configurations.
- Workflow features are grouped by capability and expose controllers, services, entities, DTOs, validation, and EF configuration from `src/Modules/Workflow`. Services use `IWorkflowDataContext`; the concrete context remains in Infrastructure.
- API authorization uses authenticated fallback plus `DomainPermissionAttribute`. Company isolation is implemented with `DataAreaId` global query filters.
- The frontend is React 19, TypeScript, MUI, TanStack Query, Zustand, React Router, and i18next. Print-template UI must remain under the existing Workflow bounded context (`src/modules/workflow`) to satisfy architecture boundaries.
- Dynamic request forms are already metadata-driven. `GET /api/v1/WfRequest/form-definition/{processId}` returns stable `requestControlId` and `controlId` values, localized labels, type, options, validation, and visibility metadata.
- The current print feature is a generic client-rendered workflow-mail report. `WorkflowMailPrintoutViewer` loads one batched mail-details model, uses `PrintoutDocument` for A4/RTL styling, and displays it in `ReportViewer`. Official output currently relies on `window.print()`; the backend has no authoritative HTML/PDF renderer.

## 2. Existing code to reuse

- `WfRequestService.GetFormDefinitionAsync`: source for valid request-control field definitions.
- The batched query pattern in `WfRequestService.GetMailDetailsAsync`: starting point for a dedicated `IPrintDataResolver`; it already loads request fields, assignments, activity details, workers, and parties without a request per field.
- `WfRequestDetail`, `WfRequestControl`, `WfControl`, `WfStep`, `WfAssignment`, and `WfActivityDetail`: normalized request/workflow sources.
- `CompanyInfo`, document-management storage, and frontend `fetchPrintoutCompany`: company identity/logo sources.
- Frontend `PrintoutDocument`, `PrintoutPaginationContext`, and `ReportViewer`: browser preview and generic-detail mode. They remain intact and are not the authoritative PDF implementation.
- Existing API client, `APIResponse<T>`, Query/notification patterns, page registry, permission guards, localization, MUI theme, and RTL Emotion cache.

## 3. Files requiring modification

Backend:

- `src/Modules/Workflow/Persistence/IWorkflowDataContext.cs`: expose print-template sets.
- `src/Modules/Workflow/WorkflowModule.cs`: register template services and validators.
- `src/Infrastructure/Persistence/ApplicationDbContext.cs`: expose the module entities.
- `src/Infrastructure/Migrations/*`: add the schema migration and snapshot changes.
- Later phases: `WfRequestController`/request-finalization flow will pin a published template version when a request becomes final.

Frontend:

- `src/modules/workflow/routes/workflowRoutePaths.ts`, `pageRegistry.ts`, and navigation configuration: register administration routes without changing unrelated navigation.
- `src/core/permissions/permissions.ts`: add matching print-template permissions.
- `public/locales/en/translation.json` and `ar/translation.json`: identical key structure.
- The existing workflow print command will later add Official Form, Full Transaction Details, and Workflow History choices while preserving the current generic printout.

## 4. New files to create

Backend, under `src/Modules/Workflow/PrintTemplates`:

- Entities and EF configurations for templates, immutable versions, and request-version bindings.
- Strongly typed template JSON contracts (no `dynamic`, no arbitrary executable expressions).
- DTOs, FluentValidation validators, `IPrintTemplateService`, implementation, and controller.
- Later: `IPrintDataResolver`, condition evaluator, validator, HTML renderer, and a PDF provider contract/implementation.

Frontend, under `src/modules/workflow/print-templates`:

- `types/printTemplate.types.ts`, `api/printTemplateApi.ts`, queries/hooks, template list page, designer page, preview page, and element renderers.
- Designer-specific state remains local/feature-owned; server results remain in TanStack Query.

## 5. Proposed database changes

`ReportTemplates`:

- `RECID` (TemplateId), `RefTableId`, `RefRecId`, `Code`, `Name`, `Description`, `PageSize`, `Orientation`, `Language`, `IsDefault`, `Status`, `CurrentVersionId` (nullable), plus standard audit, soft-delete, row-version, active, and `DataAreaId` fields.
- Unique company-scoped indexes for `(DataAreaId, RefTableId, RefRecId, Code)` and one filtered default template per referenced record.

`ReportTemplateVersions`:

- `RECID` (TemplateVersionId), `TemplateId`, `VersionNo`, `TemplateJson`, `IsPublished`, `PublishedBy`, `PublishedAt`, plus standard audit/company fields.
- Unique `(DataAreaId, TemplateId, VersionNo)`. Published version JSON is immutable; edits create/update an unpublished draft or create the next draft after publication.

`ReportEntityVersions`:

- `RECID`, `RefTableId`, `RefRecId`, `TemplateId`, `TemplateVersionId`, `SelectedAt`, `SelectedBy`, and company/audit fields.
- Unique `(DataAreaId, RefTableId, RefRecId, TemplateId)`. This is the historical pin used for reprints; a finalized entity never silently moves to a newer version.

All foreign-key deletes use restrict/no-action to match the repository's no-cascade convention. Templates and versions are company-scoped through existing global filters.

## 6. Proposed `TemplateJson` schema

The root is a versioned, discriminated document:

```json
{
  "schemaVersion": 1,
  "language": "ar",
  "direction": "rtl",
  "page": {
    "size": "A4",
    "orientation": "portrait",
    "margins": { "top": 15, "right": 15, "bottom": 15, "left": 15 }
  },
  "header": [],
  "sections": [],
  "footer": [],
  "missingFieldBehavior": "empty"
}
```

Elements use a required `id` and `type` discriminator. Bindings are explicit unions: system key, company key, stable `requestControlId`/`controlId`, workflow `stepId`, attachment source, or repeating source. Conditions are data-only `{ field, operator, value }` trees using an allow-list of operators; stored JavaScript and raw executable code are forbidden. Formatting is a typed object for date/date-time/number/currency/percentage/boolean. Layout uses section/row/column structure; absolute coordinates are not part of schema version 1.

## 7. Proposed backend flow

1. Authorize request access and company scope before resolving or printing.
2. Resolve template: explicit template, pinned request version, or active default.
3. Validate that the selected version is published for official output.
4. `IPrintDataResolver` loads request, controls/details, company, assignments/steps, and attachments in bounded batched queries and returns one normalized model.
5. `IPrintTemplateValidator` validates schema, bindings, step IDs, data sources, conditions, defaults, and supported elements.
6. `IPrintTemplateRenderer` consumes only template JSON plus the normalized model and produces deterministic HTML.
7. A PDF provider renders that HTML with embedded fonts/assets, A4 settings, RTL direction, repeated headers, and page numbering.
8. Finalization stores `ReportEntityVersions`; reprint uses that immutable version.

Controllers remain thin. Publishing, default-template uniqueness, immutability, and version creation belong in the service transaction.

## 8. Proposed frontend flow

1. Administration list filters templates by process and displays draft/published/archived state and default marker.
2. Designer loads field catalogs from the process definition and workflow-step endpoints; users select fields rather than typing property paths.
3. Designer edits a typed schema using structured sections/rows/columns. The canvas and properties panel are separate small components.
4. Preview requests one normalized print-data payload for a real/sample request and renders locally with the same schema semantics.
5. Publish first runs local validation, then authoritative server validation and version creation.
6. Normal request printing offers Official Form, Full Transaction Details, and Workflow History. One default official template opens directly; multiple active templates open a selector.
7. Official PDF download uses the backend endpoint. Browser printing remains an optional preview action.

## 9. Risks and compatibility concerns

- Current `WfRequestController` exposes request details under authentication but does not apply an explicit per-request access policy. Official print endpoints must validate ownership/assignment/admin permission, not only possession of a request ID.
- `WfRequestDetail.ControlId` is a byte while template bindings need the long `WfRequestControl.RecId`; legacy `ControlDataId` semantics must be verified and normalized before Phase 3.
- Current request completion has no single audited finalization hook. Version pinning must be placed at the actual completion transaction, with a safe lazy-pin fallback for already-finalized legacy requests.
- The current client report estimates pages from DOM height and `window.print()`; it cannot guarantee deterministic PDF pagination. It is preview infrastructure only.
- Mixed Arabic/English, commercial font licensing, QR/barcode assets, remote images, signatures, and attachment URLs need controlled embedding for reproducible PDFs.
- Repeating rows need explicit page-break tests with 1/10/100 rows; browser layout and server renderer must share conformance fixtures to prevent preview/final drift.
- A published schema must have a supported `schemaVersion`; migrations should add new readers rather than mutate historic JSON.
- Existing working generic print and transaction pages must remain unchanged until Official Form is available and authorized.

## Incremental delivery

- Phase 1: persistence, typed JSON foundation, CRUD, draft/publish/version rules, process mapping, permissions, and tests.
- Phase 2: structured A4 designer and interactive preview.
- Phase 3: normalized data resolver, field catalogs, formatting, and safe conditions.
- Phase 4: repeating tables, workflow approvals, signatures, QR/barcode, attachments, headers/footers, and page breaks.
- Phase 5: authoritative server HTML/PDF generation and print-mode integration.

## Phase 1 implementation status

Implemented: company-scoped template/version/request-pin entities, EF configurations and migration, typed polymorphic JSON contracts, safe schema validation, process/control/step validation, transactional draft/publish/archive/delete rules, CRUD endpoints, permissions, responsive administration UI, matching English/Arabic localization keys, and focused backend/frontend tests.

Deferred by design: the drag-and-drop designer, normalized request-data resolver, interactive A4 renderer, advanced print elements, finalization hook, and server PDF provider belong to Phases 2–5. The existing workflow printout remains unchanged until those paths have conformance and authorization coverage.
