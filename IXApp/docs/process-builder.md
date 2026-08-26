# Process Builder

The Workflow-owned feature under `src/modules/process-builder` is the workflow document designer. Its established package path remains stable for compatibility. It is separate from the generic `src/patterns/process-builder` presentation component and directly integrates with Workflow APIs as part of the same bounded context.

## Composition

- `pages/ProcessBuilderPage.tsx` loads/saves a builder, owns shell pane visibility, export/create commands, and restores navigation state.
- `store/useProcessBuilderStore.ts` owns the editable process document, selection, active left/center tabs, dirty state, reordering, and persisted-ID remapping.
- `api/processBuilderApi.ts` maps workflow DTOs to the builder model and coordinates multi-entity saves.
- `ProcessBuilderTreePanel`, `ProcessBuilderPalette`, `ProcessBuilderWorkspace`, and `ProcessBuilderSettingsPanel` form the navigation, palette, center workspace, and context settings panes.
- `useProcessBuilderDraft` normalizes and stores browser drafts.

## Workspaces

The center tabs are ordered: Designer, Variables, Request Form, Steps, Activities, Activity Form, Transitions, Diagram. Selection is normalized when switching tabs so the settings pane follows the active workspace. Active tab, left tab, selected step/activity/control/transition are restored from session storage per builder ID after reload.

Request Form supports all control palette types. Manual dropdown, checkbox-list, and radio-button-list controls expose inline option add/edit/delete and drag reordering. Database dropdowns do not expose manual options. Activity Form has an activity selector and persists activity controls, options, and validations.

## Backend integration

The load operation combines process, variable, step, activity, activity-control, request-control, option, validation, transition, operator, and lookup data. Saves are split into process metadata, variables, steps/activities, request controls, activity controls, and transitions so generated numeric IDs can be remapped before dependent records are written.

Backend activity classification (`NORMAL`/`PARTIAL`) is distinct from the designer behavior mode (`approval`, `review`, `data-entry`, `api`, `notification`). An explicit backend activity type ID wins; new activities fall back to the active `NORMAL` type when no matching code/name exists.

## Persistence and testing

Browser draft persistence is not a backend save and does not clear server dirty state. Request and activity options are saved with explicit sort order. Integration behavior is covered by `src/test/modules/ProcessBuilderActivityFormApi.test.ts` and UI/state behavior by `src/test/modules/ProcessBuilderPage.test.tsx`; responsive behavior is covered by `e2e/process-builder-responsive.pw.ts`.
