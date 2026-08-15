# IXApp frontend

IXApp is the React frontend for IAX. It uses React 19, TypeScript 6, Vite 8, Material UI 9, React Router 7, TanStack Query, Zustand, React Hook Form, Zod, and i18next.

The interface implements reusable enterprise page patterns, a responsive application shell, English/Arabic localization, permission-aware routes and actions, typed API adapters, and an optional mock-data mode.

## Start locally

Requirements: a current Node.js LTS release and npm.

```bash
npm install
npm run dev
```

The checked-in development environment calls `http://localhost:33319/api` and does not enable mock mode. Change local Vite environment values as needed; never put secrets in frontend environment files.

Only these variables are read by the runtime environment module:

| Variable | Purpose |
| --- | --- |
| `VITE_API_BASE_URL` | Base URL passed to the Axios API client |
| `VITE_ENABLE_MOCK_API` | Uses available mock adapters when exactly `true` |

`VITE_APP_NAME` is present in the environment files but is not currently consumed by `src/core/configuration/environment.ts`.

## Commands

| Command | Purpose |
| --- | --- |
| `npm run dev` | Start Vite development mode |
| `npm run build` | Type-check and create a production bundle |
| `npm run typecheck` | Run TypeScript without emitting files |
| `npm run lint` | Run ESLint |
| `npm run test:run` | Run the Vitest suite once |
| `npm run test:e2e` | Run Playwright through the project wrapper |
| `npm run audit:architecture` | Check source-layer and import boundaries |
| `npm run audit:encoding` | Check source encoding |
| `npm run verify` | Run the repository's full validation chain |

## Documentation

Start with the [frontend documentation index](docs/README.md). It links to architecture, routing, UI standards, shared components, forms, the custom data grid, API/state management, authentication, page patterns, Process Builder, testing, and development conventions.

The intended dependency model and the currently reported exceptions are recorded in [Architecture boundaries](docs/ARCHITECTURE-BOUNDARIES.md). Pattern maturity is listed in [Page patterns](docs/patterns.md); scaffold folders are not presented as finished implementations.

## Source map

```text
src/
├── app/       # bootstrap, providers, routes, layouts, shell, theme, app stores
├── core/      # API, auth, errors, localization, permissions, configuration
├── shared/    # reusable components, hooks, services, types, utilities
├── patterns/  # reusable page-level compositions and pattern scaffolds
├── modules/   # business features and Process Builder
├── mocks/     # shared mock datasets
└── test/      # Vitest tests and test utilities
```

See [Architecture](docs/app.md), [Modules](docs/modules.md), and [Development guidelines](docs/development.md) before adding a feature.
