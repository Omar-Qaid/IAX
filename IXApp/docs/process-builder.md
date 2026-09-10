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

Variable data types use the same `WfDataType` catalog API as the Workflow Variables page. Process Builder resolves `INT`, `STR`, `DT`, and `BOOL` by code rather than assuming numeric IDs. Unknown codes and unsupported Object mappings fail explicitly; variable saves validate all type mappings before deleting or writing variable rows. Existing type IDs are retained when their semantics are unchanged, including inactive types already in use.

Transition operators also resolve by semantic code. A missing code may use a supported legacy name/symbol; an unknown code is rejected instead of becoming equality. Changing a comparison resolves a matching catalog ID, and ambiguous or unavailable mappings fail before transition mutations.

Browser drafts carry a data-type catalog version marker. For older drafts, existing variables recover their type from the server while other edits and new variables are retained. Any intentional type edit in an older draft must be selected again; drafts created with the marker preserve type edits normally.

Browser draft persistence is not a backend save and does not clear server dirty state. Request and activity options are saved with explicit sort order. Integration behavior is covered by `src/test/modules/ProcessBuilderActivityFormApi.test.ts` and UI/state behavior by `src/test/modules/ProcessBuilderPage.test.tsx`; responsive behavior is covered by `e2e/process-builder-responsive.pw.ts`.

### Configurable date bounds

In a control's Validation pane, add **Minimum Date** and/or **Maximum Date** rules. Enter the bound in Value:

- `today`: current UTC calendar date.
- `today+1y`, `today+6m`, `today-30d`: signed offsets in calendar years, months or days.
- `2026-01-01`: a fixed ISO date.

For dates from today through one year ahead, configure Minimum Date = `today` and Maximum Date = `today+1y`. Bounds are inclusive; a maximum date includes that entire day for date-time inputs. Month/year offsets clamp to the last valid day of the target month. Date rules do not apply to standalone time-only pickers.

These rules use existing request/activity validation rows and survive adapter save/load. Error-severity bounds set native date/date-time picker limits and show input-time errors. Request submission and the request validation engine enforce the same UTC calendar semantics on the server. Optional empty values remain allowed unless a Required rule applies. Malformed bound expressions fail configuration validation; Builder form saves reject reversed active error bounds. No schema migration is needed.

Date arithmetic, picker attributes, manual input rejection and adapter payloads have automated coverage. Native calendar appearance and authenticated database round trips have not been verified. Updated server validation requires the rebuilt API to be running.
