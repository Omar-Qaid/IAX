# IXApp frontend architecture

This file is a short compatibility entry point. The maintained documentation starts at [docs/README.md](docs/README.md); topic-specific documents are the source of truth instead of duplicating their content here.

## Runtime composition

`src/main.tsx` mounts `App`. `AppProviders` composes error, localization, query, authentication, theme, and notification providers, and `AppRoutes` renders the route tree. Lazy page registrations live in `src/app/routes/pageRegistry.ts`; route composition lives in `src/app/routes/routeConfig.tsx`.

Read [Application bootstrap](docs/app.md), [Routing and layouts](docs/routing-and-layouts.md), and [Authentication and authorization](docs/authentication.md) for the verified flow.

## Layers

| Layer | Current responsibility |
| --- | --- |
| `app` | Bootstrap, providers, routing, layouts, shell/navigation, theme, application UI stores |
| `modules` | Business features, pages, APIs/adapters, queries, validation, and feature state |
| `patterns` | Reusable page compositions without business ownership; several folders remain scaffolds |
| `shared` | Route-agnostic components, hooks, services, validation, types, and utilities |
| `core` | API/auth/error/localization/permission infrastructure and generic contracts |
| `mocks` | Shared mock datasets used by the adapters that support mock mode |

The precise allowed dependency matrix is in [Architecture boundaries](docs/ARCHITECTURE-BOUNDARIES.md). The architecture audit currently reports a small set of upward-layer and cross-module imports. Those imports are implementation debt, not approved exceptions; documentation must not claim the audit is clean until the code is changed.

## Technology and implementation choices

- React 19 and TypeScript 6 on Vite 8.
- Material UI 9; the main table is a project-owned virtualized grid using MUI and TanStack Virtual, not MUI X Data Grid.
- TanStack Query for server state, Zustand for selected application and feature state, and local React state where ownership is local.
- React Hook Form and Zod for implemented form flows.
- Axios-based API infrastructure with HTTP and mock adapters selected by feature services.
- React Router 7 with lazy page registration and guarded protected routes.
- i18next with English/Arabic resources and LTR/RTL theme direction.
- Vitest/Testing Library for component and integration tests, plus Playwright browser tests.

## Working conventions

- Inspect an existing feature and its documentation before introducing a new pattern.
- Keep HTTP calls in API/adaptor/query layers rather than view components.
- Keep server results out of Zustand unless a feature explicitly owns a client-side editing model.
- Use the shared fields, forms, page primitives, feedback states, dialogs, and custom data grid where their contracts fit.
- Import Material UI icons from their individual paths.
- Add translated user-facing text and verify RTL-sensitive layouts.
- Do not describe scaffold-only patterns as implemented.

See [Development guidelines](docs/development.md), [UI/UX and responsive standards](docs/ui-ux-and-responsive.md), and [Testing](docs/testing.md) for actionable guidance.
