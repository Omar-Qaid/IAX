# Shared Layer Master Documentation (`src/shared`)

## 1. Purpose and Responsibilities
The `shared` layer contains reusable UI controls, generic enterprise components, form fields, virtualized data grids, dialogs, action panes, FastTabs, lookups, logistics drawers, and generic hooks for **IXApp**.

All components in the `shared` layer are **100% domain-agnostic**. They must never contain hardcoded business rules, domain-specific endpoints, or module-specific state.

---

## 2. Dedicated Sub-Folder Documentation Index

Every component sub-folder inside `src/shared/` has a dedicated documentation specification:

| Sub-Folder / Component | Dedicated Documentation File | Description |
|---|---|---|
| **Action Pane** | [`docs/shared/action-pane.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/shared/action-pane.md) | D365 F&O-style grouped action toolbars & RBAC command guards |
| **App Shell** | [`docs/shared/app-shell.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/shared/app-shell.md) | Application shell, topbar, sidebar, command palette (`Ctrl+K`), notification drawer |
| **Data Grid** | [`docs/shared/data-grid.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/shared/data-grid.md) | `AppDataGrid` virtualized data table, inline editing, persistence, CSV export |
| **Dialogs** | [`docs/shared/dialogs.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/shared/dialogs.md) | Modal dialog container (`AppDialog`), confirmation & delete warning dialogs |
| **FastTabs** | [`docs/shared/fast-tabs.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/shared/fast-tabs.md) | Collapsible form section accordions with summary text & error chips |
| **Feedback States** | [`docs/shared/feedback.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/shared/feedback.md) | Standardized loading, empty result, error alert, and access denied states |
| **Form Fields** | [`docs/shared/fields.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/shared/fields.md) | React Hook Form input controls (`AppTextField`, `AppSelectField`, `AppLookupGridField`) |
| **Form Layouts** | [`docs/shared/forms.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/shared/forms.md) | 12-column responsive layout wrappers (`FormRow`, `FormColumn`) & error banners |
| **Logistics Drawers** | [`docs/shared/logistics.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/shared/logistics.md) | Slide-out right drawers for postal addresses & electronic contact channels |
| **Grid Lookups** | [`docs/shared/lookups.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/shared/lookups.md) | Virtualized multi-column popover table dropdowns & RBAC field guards |
| **Page Layouts** | [`docs/shared/page.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/shared/page.md) | Structural page containers (`PageContainer`, `PageHeader`, `PageContent`, `PageSection`) |
| **Shared Hooks** | [`docs/shared/hooks.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/shared/hooks.md) | Generic hooks (`useNotifications`, `useDebounce`, `useLogisticsAddress`, `useLookupGridField`) |
| **Shared Utilities** | [`docs/shared/utilities.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/shared/utilities.md) | Helper functions for column localization, action filtering, and grid exports |

---

## 3. Strict Architecture & Dependency Rules
- **Allowed Dependencies:** `@shared` $\rightarrow$ `@core`.
- **Forbidden Dependencies:** `@shared` MUST NOT import from `@patterns`, `@modules`, or `@app`.
- **Vitest ESM Icon Import Rule:** Icons MUST NOT be imported from `@mui/icons-material` barrel index. Always use specific path imports (`import AddIcon from '@mui/icons-material/Add'`).
