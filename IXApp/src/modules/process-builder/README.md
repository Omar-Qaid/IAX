# Process Builder module

## Purpose

Provides the full-screen workflow designer that edits process metadata, variables, request forms, steps, activities, activity forms, transitions, and diagram-oriented state.

## Structure

- `pages/ProcessBuilderPage.tsx`: route-level orchestration and tab/selection persistence.
- `components/ProcessBuilderWorkspace.tsx`: designer workspace and tab content.
- `components/ProcessBuilderPalette.tsx`, `ProcessBuilderTreePanel.tsx`, `ProcessBuilderSettingsPanel.tsx`: palette, hierarchy, and selected-item configuration.
- `components/ConditionBuilder.tsx`, `ControlPreview.tsx`, sortable item components: control/condition editing and drag ordering.
- `hooks/useProcessBuilderDraft.ts`: draft lifecycle/integration hook.
- `store/useProcessBuilderStore.ts`: feature Zustand editing and selection state.
- `api/processBuilderApi.ts`: Process Builder transport helpers.
- `types/processBuilderTypes.ts`: draft and builder contracts.

The module currently orchestrates Workflow APIs/components directly, which the architecture audit flags as cross-module debt. This README records that implementation; it does not authorize more cross-module imports.

State flows from the selected process/step/activity into the Zustand draft, editor panels, and save APIs. UI tab, step, and activity selections are persisted so reload can restore context.

[Process Builder integration](../../../docs/process-builder.md) · [Workflow](../workflow/README.md) · [Architecture boundaries](../../../docs/ARCHITECTURE-BOUNDARIES.md)
