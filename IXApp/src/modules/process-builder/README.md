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

Process Builder is owned by Workflow and retains its established `src/modules/process-builder` package path. Its Workflow API integration follows the ownership boundary documented in the Process Builder integration guide. The generic `src/patterns/process-builder` remains a separate presentation component.

State flows from the selected process/step/activity into the Zustand draft, editor panels, and save APIs. UI tab, step, and activity selections are persisted so reload can restore context.

[Process Builder integration](../../../docs/process-builder.md) · [Workflow](../workflow/README.md) · [Architecture boundaries](../../../docs/ARCHITECTURE-BOUNDARIES.md)
