# Page primitives

`src/shared/components/page` contains route-agnostic composition pieces:

- `PageContainer`: vertical root layout.
- `PageHeader` and `PageTitle`: title, subtitle, badge, and action slots.
- `PageContent`: bordered Paper content surface.
- `PageSection`: titled/optional-description section.
- `PageSummary` and `PageStatusBar`: summary and status regions.
- `EnterpriseListHeader`: context/view heading for enterprise pages.
- `RightUtilityRail` and `RelatedInformationPanel`: filter/information side surfaces.
- `UnsavedChangesGuard`: renderless browser-unload protection.

`PageContent` and `PageSection` are implemented in their own files; older documentation claiming they were defined inside `PageContainer.tsx` is obsolete. Route-aware breadcrumbs belong in `app/routes/routeMetadata.ts` and the shell, not in shared page components.

Patterns may choose a full-height specialized root instead of `PageContainer`, but normal routed pages should prefer these primitives for consistent semantics and spacing.
