# Frontend source

## Overview

`src` contains the IXApp runtime and its Vitest suites. `app/main.tsx` is the browser entry point; business behavior is organized below it by architectural ownership.

| Folder | Responsibility | Documentation |
| --- | --- | --- |
| `app` | Bootstrap, providers, routes, layouts, shell, theme, and application UI stores | [README](app/README.md) |
| `assets` | Images imported by source code | [README](assets/README.md) |
| `core` | API, authentication, errors, localization, permissions, and generic infrastructure | [README](core/README.md) |
| `mocks` | Shared in-memory datasets | [README](mocks/README.md) |
| `modules` | Business and feature modules | [README](modules/README.md) |
| `patterns` | Reusable page-level compositions and pattern scaffolds | [README](patterns/README.md) |
| `shared` | Route-agnostic UI, hooks, services, types, validation, and utilities | [README](shared/README.md) |
| `test` | Vitest tests, setup, and render helpers | [README](test/README.md) |

## Dependency model

The intended direction is `app → modules → patterns → shared → core`, with additional rules enforced by `scripts/audit-architecture.mjs`. The audit currently reports documented implementation debt; see [Architecture boundaries](../docs/ARCHITECTURE-BOUNDARIES.md).

## Adding functionality

Assign ownership before adding files: business behavior belongs to a module, reusable page composition to a pattern, reusable route-agnostic UI to shared, and application composition to app. Register pages through the app route registry and keep API access behind feature/core service boundaries.

## Related documentation

- [Frontend documentation](../docs/README.md)
- [Architecture](../docs/app.md)
- [Development guidelines](../docs/development.md)
