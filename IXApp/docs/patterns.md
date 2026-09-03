# Page patterns (`src/patterns`)

Patterns are domain-neutral page compositions. A pattern is only considered implemented when its source exports working UI; the presence of a folder alone is not implementation.

## Current status

| Pattern folder    | Status                           | Actual role                                                                                                                                                                     |
| ----------------- | -------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `simple-list`     | Implemented                      | Standard or enterprise list with static, controlled, or remote data sources; grid editing, search/filter, utilities, and related information.                                   |
| `list-details`    | Implemented                      | Legacy split view and enterprise record browser/details editor with remote/controlled/static sources, permissions, validation, number sequences, panels, and optional resizing. |
| `master-form`     | Implemented                      | Thin `PageContainer`/header/action-pane/Paper composition plus local `useMasterFormPage`.                                                                                       |
| `workspace`       | Implemented                      | Page wrapper, section, and KPI tile primitives.                                                                                                                                 |
| `document`        | Implemented                      | Header, lines, optional responsive totals, dialogs, and a small document-state hook.                                                                                            |
| `setup`           | Implemented                      | Config-driven navigation, accordion fields, local dirty/save state, and responsive split layout.                                                                                |
| `lookup`          | Implemented                      | Full-page client-filtered grid lookup with double-click selection.                                                                                                              |
| `tabbed-details`  | Implemented                      | Scrollable exclusive tabs with controlled notification callback.                                                                                                                |
| `process-builder` | Implemented presentation pattern | Generic three-pane structure/tree/tabs; distinct from the workflow module's full Process Builder feature.                                                                       |
| `report-designer` | Implemented workspace pattern    | Accessible bounded toolbar/workspace shell; document schema, bindings, canvas, properties, and persistence remain feature-owned.                                                |
| `inquiry`         | Empty scaffold                   | `InquiryPage.tsx` and `types.ts` contain no implementation.                                                                                                                     |
| `master-detail`   | Partial scaffold                 | `MasterDetailLayout.tsx` exists; page, types, and hook are empty.                                                                                                               |
| `process`         | Partial scaffold                 | Navigation/indicator files exist; page and types are empty.                                                                                                                     |
| `profile`         | Partial scaffold                 | Header/summary files exist; page and types are empty.                                                                                                                           |
| `tree-details`    | Partial scaffold                 | `TreeNavigation.tsx` exists; page and types are empty.                                                                                                                          |

There is no `patterns/dashboard` or `patterns/master-detail-grid` source folder. Dashboard modules use the workspace pattern; the similarly named legacy docs explain this status rather than claiming an implementation.

## Selection guidance

- Flat reference data: `SimpleListPage`.
- Rich record browser plus editable detail sections: enterprise `ListDetailsPage`.
- Singleton form container: `MasterFormPage` or config-driven `SetupPage`.
- Header/lines/totals transaction: `DocumentPage`.
- KPI/operational landing page: `WorkspacePage`.
- Full-page local lookup or exclusive tabs: `LookupPage` or `TabbedDetailsPage`.
- Specialized cross-entity designer: a module-owned page, such as Process Builder.
- Visual report/template editor: `ReportDesigner`, composed with a feature-owned document model and persistence.

Do not select an empty scaffold for production behavior without implementing and testing it first. Pattern-specific notes are in [`docs/patterns`](patterns/).
