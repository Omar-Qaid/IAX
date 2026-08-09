# Architecture Boundaries and Migration Baseline

This document defines the enforceable dependency model for `src/`. It complements
`ARCHITECTURE.md` and records temporary debt without redefining that debt as an
accepted design.

## Dependency matrix

| Source layer       | May import                                              |
| ------------------ | ------------------------------------------------------- |
| `app`              | `app`, `modules`, `patterns`, `shared`, `core`, `mocks` |
| `modules/<module>` | Its own module, `patterns`, `shared`, `core`, `mocks`   |
| `patterns`         | `patterns`, `shared`, `core`                            |
| `shared`           | `shared`, `core`                                        |
| `core`             | `core` and external packages                            |
| `mocks`            | `mocks`, `shared`, `core`                               |
| `test`             | Every source layer                                      |

Business modules must not import another business module directly. Integration
between modules belongs in `app` composition or in a deliberately shared/core
contract that has no dependency on either business module.

## Ownership decisions

- `app` owns bootstrap, providers, routes, application layouts, shell composition,
  navigation configuration, route-aware feedback, global UI stores, and theme
  composition.
- `core` owns framework-independent infrastructure: API transport, authentication
  contracts and storage adapters, authorization logic, localization setup, error
  normalization, and pure utilities. It must not render application-specific UI.
- `shared` owns reusable, route-agnostic UI and hooks. Shared APIs receive labels,
  navigation callbacks, configuration, and state through props or narrow contracts.
- `patterns` owns reusable page-level workflows composed only from `shared` and
  `core` capabilities.
- `modules` own business pages, domain components, services, queries, DTOs, schemas,
  and module-specific mock adapters.
- `identity` owns authentication-facing pages. `finance` is the parent bounded context
  for `foundation` and `accounts-receivable`; those subdomains may share only
  deliberately finance-owned contracts.
- `mocks` supplies development implementations and fixtures; production pages must
  not select mocks directly.

## Current migration baseline

The architecture audit permits zero forbidden layer edges and zero MUI icon-barrel
imports. Any future exception must be listed explicitly in
`scripts/audit-architecture.mjs`; broad directory exceptions are not permitted.

The baseline is a ratchet:

- A new forbidden layer edge fails the audit.
- A direct cross-module import fails the audit with no baseline allowance.
- A new `@mui/icons-material` barrel import fails the audit.
- Any file-level circular dependency fails the audit.
- When a known violation is removed, the audit reports the stale baseline entry so
  it can be deleted in the same change.

## Planned removal order

1. ~~Move application shell and route-aware navigation from `shared` to `app`.~~ Completed.
2. ~~Move route-specific access-denied rendering into `app/routes` and leave pure
   authorization logic in `core`.~~ Completed.
3. ~~Move validated environment ownership into `core/configuration` so authentication
   and generic data hooks do not import `app`.~~ Completed.
4. ~~Move logistics mock selection behind a shared service adapter.~~ Completed.
5. ~~Replace each known MUI icon barrel import with a path import.~~ Completed.

Each wave must preserve compatibility until all consumers are migrated and must run
`npm run verify` plus Playwright discovery and assertions before its baseline entries
are removed.

## Unused-export report

`npm run audit:unused` reports possible unused exports. It is non-blocking because
text-based export analysis can produce false positives for dynamic loading, public
package APIs, declaration merging, and framework conventions. No export may be
deleted without checking runtime routes, barrel exports, tests, and documentation.
