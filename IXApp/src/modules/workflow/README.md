# Workflow module

## Purpose

Owns workflow setup pages and typed API wrappers for categories, processes, steps, variables, activities, performers, controls/options/validations, transitions, request controls, print templates, and master/setup data.

## Pages

`WFCategoryPage`, `WFProcessPage`, `WFStepsPage`, `WFVariablesPage`, `WfActivitiesPage`, `WfControlsPage`, `WfActivityTypesPage`, `WfDataTypesPage`, and `WfPrioritiesPage` are registered by the app page registry. Repeated setup lists use `components/WorkflowSetupListPage.tsx`. `lookups/processLookup.ts` provides process lookup configuration, and `routes/workflowRoutePaths.ts` holds workflow-specific path constants.

`print-templates` owns the process-scoped template administration UI, API contract, and strongly typed versioned document schema. Keep designer, preview, and field-catalog additions inside this feature folder; do not duplicate print-template state in Process Builder.

## API and data flow

Page → module API wrapper → core Axios client → backend. Each file under `api` owns its named workflow resource; option and validation resources have separate wrappers. Process Builder currently consumes several of these wrappers directly.

Keep backend field names and option ordering contracts in the typed API layer. Use shared feedback, forms, grid, and lookup components for page presentation.

[Modules](../README.md) · [Process Builder](../process-builder/README.md) · [API and state](../../../docs/api-and-state.md)
