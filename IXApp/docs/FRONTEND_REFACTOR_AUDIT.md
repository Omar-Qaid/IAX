# Frontend refactoring audit

## Scope and baseline

The repository contains a React 19, TypeScript, Vite, Material UI, TanStack Query,
React Hook Form, Zod, Zustand, i18next, and Vitest frontend. No backend project is
present under `IXApp`, and this migration does not change API endpoints or payloads.

The source inventory contains 328 TypeScript source files (172 TSX and 150 TS).
The intended dependency direction is `app -> modules -> patterns -> shared -> core`.
At audit time, type checking passed, lint passed with two React Compiler compatibility
warnings from TanStack Virtual, and the routed business pages were placeholders.

## Page-pattern classification

| Route/page | Module | Current pattern | Target pattern | Shared components | Main issues | Risk |
| --- | --- | --- | --- | --- | --- | --- |
| Login | Authentication | Placeholder | Master Form | AuthLayout, EntityForm, fields, feedback | No real authentication form | High |
| Dashboard | Dashboard | Placeholder | Workspace | WorkspacePage, WorkspaceTile, WorkspaceSection | No overview content | Low |
| Customers | Accounts Receivable | Placeholder | List and Details | ListDetailsPage, DataGrid, FastTabs, StatusBadge | No service-backed list/detail controller | Medium |
| Customer Groups | Accounts Receivable | Placeholder | Simple List | SimpleListPage, DataGrid | No page implementation | Low |
| Sales Orders | Accounts Receivable | Placeholder | Simple List | SimpleListPage, DataGrid, StatusBadge | No list page or navigation to document | Low |
| Sales Order Details | Accounts Receivable | Placeholder | Header-Lines Document | DocumentPage, DataGrid, StatusBadge | Document is not service-backed; mutation workflow absent | High |
| Currencies | Foundation / General Ledger | Placeholder | Simple List | SimpleListPage, DataGrid | No page implementation | Low |
| Application Settings | System Administration | Placeholder | Setup / Parameters | SetupPage, FastTabs, fields | No backend settings contract | Medium |

No routed pages currently implement Lookup, Inquiry, Process, Tree and Details,
Tabbed Details, Profile, Master Form, or Master-Detail patterns. Pattern scaffolds exist
for most of them, but several are placeholders and must not be treated as production-ready.

## Duplicate and inconsistency report

- `PageContent` and `PageSection` are implemented inside `PageContainer.tsx` while
  same-named files contain placeholder components. This creates ambiguous ownership.
- `useDocumentPage` exists in both `patterns/document` and `shared/hooks`.
- Lookup dialogs exist under both `shared/components/dialogs` and
  `shared/components/lookups`, with unclear ownership.
- Numerous shared and pattern components use `React.FC<any>` despite the documented
  zero-`any` rule. The grid and lookup public types also expose `any`.
- Several page-pattern support components render only placeholder text or empty divs.
- Route elements are declared inline and eagerly; business modules contain only IDs.
- Navigation advertises many paths that have no route, creating predictable dead links.
- User-facing fallback strings and several component colors are hardcoded despite the
  localization and theme-token rules.
- Mock Arabic text and currency symbols show mojibake, indicating an encoding problem.
- Documentation claims MUI X Data Grid, while the application uses a custom virtualized
  grid. The installed package and implemented architecture are therefore inconsistent.
- Auth starts with a default administrator and writes a mock token. This is development
  behavior and must not be assumed to match the backend authentication protocol.

## Target structure

The existing top-level structure is appropriate and should be retained:

```text
src/
  app/              bootstrap, providers, layouts, routes, theme, global UI state
  core/             API, auth, errors, localization, permissions, pure utilities
  shared/           reusable components, hooks, services, validation, types
  patterns/         typed page-pattern composition only
  modules/          domain pages, components, hooks, services, schemas, types
  mocks/            development-only typed repositories and datasets
```

Each business module should own `pages`, `components`, `hooks`, `services`, `types`,
and `validation` as needed. Patterns must remain domain-agnostic.

## Reusable component inventory

Available foundations include the application shell, page container/header/title,
action pane, custom data grid, forms and fields, FastTabs, dialogs, lookups, feedback,
status badges, permission guards, notifications, and unsaved-change hooks. The first
priority is to complete and type these existing foundations, not create competing ones.

Priority completion candidates are `PageSection`, `PageContent`, `WorkspaceSection`,
document subcomponents, dialog wrappers, field wrappers, grid public types, and pattern
controller hooks.

## Migration order

1. Foundation hygiene: remove placeholder primitives, eliminate duplicate ownership,
   type public APIs, centralize localization and theme tokens.
2. Low-risk read-only representatives: Dashboard, Currencies, Customers, Sales Order,
   and Settings.
3. Add domain service/query boundaries matching existing ASP.NET contracts.
4. Add create/edit/save/delete only after endpoint and validation contracts are known.
5. Migrate remaining routed lists and detail pages.
6. Validate navigation against route configuration and hide unsupported links.
7. Remove obsolete scaffolds only after all imports and tests are migrated.

## Risks

- **High:** authentication and document mutation behavior cannot be safely completed
  without the real backend contracts.
- **High:** placeholder pattern APIs may be consumed as if production-ready.
- **Medium:** custom grid complexity and pervasive `any` make editing regressions likely.
- **Medium:** navigation contains unsupported routes and permission mappings.
- **Medium:** localization encoding defects can corrupt displayed business data.
- **Low:** read-only pages over typed mock data are isolated and reversible.

## Files proposed for the first batch

- `docs/FRONTEND_REFACTOR_AUDIT.md`
- `src/shared/components/page/PageContent.tsx`
- `src/shared/components/page/PageSection.tsx`
- `src/patterns/workspace/WorkspaceSection.tsx`
- `src/modules/dashboard/pages/DashboardPage.tsx`
- `src/modules/foundation/pages/CurrenciesPage.tsx`
- `src/modules/accounts-receivable/pages/CustomerListPage.tsx`
- `src/modules/accounts-receivable/pages/SalesOrderPage.tsx`
- `src/modules/system-administration/pages/ApplicationSettingsPage.tsx`
- `src/app/routes/routeConfig.tsx`
- focused tests for the new representative pages

Files under `takeideafromhere` are outside the frontend and have pre-existing deletions;
they are intentionally excluded.
