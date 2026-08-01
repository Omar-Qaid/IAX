# IXApp Master Frontend Architecture & Single Source of Truth

## 1. Document Purpose
This document serves as the **official single source of truth** for both software engineers and AI coding assistants working on **IXApp**. It defines the overall architecture, layer responsibilities, dependency rules, design principles, coding standards, naming conventions, AI decision guidelines, and references to all folder-level documentation.

Whenever technical requirements, folder structures, component implementations, or refactoring strategies are ambiguous or conflicting, **this document and its referenced documentation files in `docs/` must be followed strictly**.

---

## 2. Executive Architecture Overview

IXApp is a high-performance, modular enterprise React.js frontend inspired by Microsoft Dynamics 365 Finance & Operations page patterns and enterprise interaction concepts. It does not copy Microsoft proprietary source code, branding, or exact visual assets. Instead, it provides a clean, scalable, maintainable architecture using:

- **React 19 & TypeScript 5**
- **Vite 8**
- **Material UI v9 & MUI X Data Grid**
- **TanStack Query (React Query v5)**
- **React Hook Form & Zod**
- **Zustand**
- **i18next**
- **Vitest & React Testing Library**

The application is engineered to connect to an ASP.NET Core REST Web API backend, operating seamlessly with typed in-memory mock repositories when `VITE_ENABLE_MOCK_API=true`.

---

## 3. Documentation System Index

Detailed, specialized documentation for every folder layer is maintained in the `docs/` directory:

| Folder / Layer | Documentation File | Description & Scope |
|---|---|---|
| **Application Layer** | [`docs/app.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/app.md) | Bootstrap, global providers, layouts, routing, theme composition, Zustand stores |
| **Core Layer** | [`docs/core.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/core.md) | Axios API client, authentication, RBAC permissions, error mapping, localization engine |
| **Shared Layer** | [`docs/shared.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/shared.md) | Action pane, shell controls, virtualized DataGrid, form fields, FastTabs, dialogs, lookups, logistics drawers |
| **Page Patterns** | [`docs/patterns.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/patterns.md) | Reusable page templates: Simple List, List & Details, Master Form, Workspace, Document |
| **Business Modules** | [`docs/modules.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/modules.md) | Domain business modules: Accounts Receivable, Dashboard, Auth, System Admin |
| **Mock Services** | [`docs/mocks.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/mocks.md) | Typed mock datasets, repository implementations, and mock/HTTP service resolvers |
| **Testing Strategy** | [`docs/testing.md`](file:///c:/Users/Omar.Qaid/Desktop/IAX/IXApp/docs/testing.md) | Vitest runner rules, JSDOM quirks, icon import rules, RTL rendering tests |

---

## 4. Layer Architecture & Responsibilities

IXApp uses a **feature-based layered modular architecture**:

```text
app
 ↓
modules
 ↓
patterns
 ↓
shared
 ↓
core
```

### Layer Responsibilities Matrix

| Layer | Path | Responsibility |
|---|---|---|
| **`app`** | `src/app/` | Application startup, providers, root layouts, router setup, global Zustand stores, theme factory |
| **`modules`** | `src/modules/` | Domain business features, business pages, domain hooks, services, validation schemas, domain types |
| **`patterns`** | `src/patterns/` | Standardized page layout templates (Simple List, List & Details, Master Form, Workspace, Document) |
| **`shared`** | `src/shared/` | Reusable generic UI components, fields, virtualized data grids, dialogs, action panes, FastTabs, lookups, logistics drawers |
| **`core`** | `src/core/` | Low-level infrastructure: Axios client, API error mapper, auth session, RBAC engine, i18n setup, pure utilities |

---

## 5. Strict Dependency Rules

To prevent spaghetti code, circular dependencies, and architectural decay, the following dependency rules are strictly enforced:

### Permitted Import Directions
- `app` $\rightarrow$ `modules`, `patterns`, `shared`, `core`
- `modules` $\rightarrow$ `patterns`, `shared`, `core`
- `patterns` $\rightarrow$ `shared`, `core`
- `shared` $\rightarrow$ `core`
- `core` $\rightarrow$ External NPM packages only

### Forbidden Import Directions
- `core` MUST NOT import from `shared`, `patterns`, `modules`, or `app`.
- `shared` MUST NOT import from `patterns`, `modules`, or `app`.
- `patterns` MUST NOT import from `modules` or `app`.
- **Cross-Module Isolation:** One business module MUST NOT import directly from another business module (`accounts-receivable` must not import from `general-ledger`).

---

## 6. Naming Conventions & Code Standards

- **React Components & Files:** `PascalCase.tsx` matching component export (e.g., `CustomerForm.tsx`, `LookupGrid.tsx`).
- **Hooks:** `camelCase.ts` starting with `use` (e.g., `useLookupGridField.ts`, `useNotifications.ts`).
- **Utilities & Services:** `camelCase.ts` (e.g., `formatUtils.ts`, `customerService.ts`).
- **Constants & Storage Keys:** `UPPER_SNAKE_CASE` (e.g., `DEFAULT_PAGE_SIZE`, `VITE_API_BASE_URL`).
- **Path Aliases:** Always use configured aliases (`@app/*`, `@core/*`, `@shared/*`, `@patterns/*`, `@modules/*`, `@mocks/*`, `@test/*`). Never use long relative path chains like `../../../../shared`.

---

## 7. AI Decision Guidelines & Assistant Rules

When an AI assistant receives requests to edit, refactor, or create code in IXApp, it **must adhere strictly** to the following decision rules:

1. **Check Existing Components First:** Never create custom helper classes or ad-hoc field controls if a generic component exists in `@shared/components` (e.g., use `AppLookupGridField` for popover grid lookups, `FastTabs` for form sections).
2. **Obey Material UI Icon Import Rule:** Never import icons from the barrel `@mui/icons-material`. Always use specific path imports (`import SearchIcon from '@mui/icons-material/Search'`) to prevent Vitest ESM test suite crashes.
3. **No Direct Axios Calls in Views:** Page components and visual components must never call Axios directly. Always route HTTP operations through domain services or TanStack Query hooks.
4. **State Tool Separation:** 
   - Server data $\rightarrow$ TanStack Query (`useQuery`, `useMutation`).
   - Form fields & validation $\rightarrow$ React Hook Form & Zod.
   - Client UI state (sidebar open, theme mode) $\rightarrow$ Zustand.
   - Do **NOT** duplicate API results or form inputs inside Zustand.
5. **Preserve Bilingual & RTL Support:** All user-facing strings must use translation keys (`useTranslation`). Components must adapt layouts when `i18n.language === 'ar'`.
6. **Theme Token Rules:** Never hardcode hex color strings (like `#000` or `#fff`) inside components. Use Material UI theme tokens (`palette.primary.main`, `palette.divider`, `palette.text.secondary`).
7. **Empirical Verification:** Never declare a task complete without executing validation commands (`npm run typecheck`, `npm run test:run`, `npm run build`).

---

## 8. Definition of Done Checklist

A feature or refactoring task is complete when:
- [ ] Code is placed in the correct architectural layer.
- [ ] Dependency direction rules are strictly respected.
- [ ] TypeScript interfaces are strongly typed with zero `any`.
- [ ] Path imports for icons are used (`@mui/icons-material/IconName`).
- [ ] All user-facing strings are localizable.
- [ ] Unit or integration tests are added/updated in `src/test`.
- [ ] `npm run typecheck` passes with **0 errors**.
- [ ] `npm run test:run` passes cleanly (**0 failed tests**).
- [ ] `npm run build` generates Vite production distribution bundle.
