# Shared layer (`src/shared`)

The shared layer contains domain-agnostic UI, hooks, services, validation helpers, types, and utilities. It may depend on `core` but not on `app`, `patterns`, or business modules.

## Component families

| Family | Guide | Notable exports |
| --- | --- | --- |
| Action pane | [action-pane.md](shared/action-pane.md) | `ActionPane`, groups, buttons, menus, enterprise CRUD/utilities |
| Common | this page | `AppIconButton`, `AppMenu`, `AppTooltip`, `AsyncBoundary`, `ResponsiveStack`, `VisuallyHidden` |
| Data grid | [data-grid.md](shared/data-grid.md) | `DataGrid`/`AppDataGrid`, toolbar, mobile body, sidebar, grid hooks |
| Dialogs | [dialogs.md](shared/dialogs.md) | base, confirmation, delete, form, process, history, lookup dialogs |
| FastTabs | [fast-tabs.md](shared/fast-tabs.md) | `FastTabs`, `FastTab`, header and summary |
| Feedback | [feedback.md](shared/feedback.md) | loading, error, empty, no-results, access-denied, alert/notification |
| Fields | [fields.md](shared/fields.md) | text, number, currency, date/time, select, enum, lookup, generated code |
| Forms | [forms.md](shared/forms.md) | layout, entity form, validation summaries, local entity-form hook |
| Logistics | [logistics.md](shared/logistics.md) | postal and electronic-address drawers |
| Lookups | [lookups.md](shared/lookups.md) | virtualized grid lookup, form wrapper, dialog lookup |
| Page | [page.md](shared/page.md) | page structure, related information, utility rail, unsaved guard |
| Status | this page | `StatusBadge`, `RecordStatus`, `DocumentStatus` alias |

## Other shared areas

- `hooks`: documented in [hooks.md](shared/hooks.md).
- `services`: notifications, preferences/storage, and logistics fixture adapter.
- `validation`: common Zod schemas, messages, and issue mapping.
- `types`: actions, forms, logistics, navigation, page, and record contracts.
- `utilities` and `utils`: pure UI/domain-neutral helpers, documented in [utilities.md](shared/utilities.md).

Shared components are not universally mandatory. Use them when their public contract fits; specialized pattern internals may use MUI primitives directly. Keep business labels, endpoints, permissions, and data mapping at the caller boundary.
