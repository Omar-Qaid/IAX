# Frontend tests

Vitest setup is in `setupTests.ts`; `testUtils.tsx` supplies common rendering/provider helpers. Tests are organized by ownership:

| Folder | Coverage focus |
| --- | --- |
| `app` | Providers, shell, navigation, routes, metadata, density |
| `core` | Auth sessions, errors, localization, permissions, utilities |
| `mocks` | Mock service behavior |
| `modules` | Administration, organization, Process Builder, Workflow, representative pages |
| `patterns` | Page patterns and generic Process Builder |
| `shared` | Grid, fields, dialogs, lookups, logistics, notifications, hooks, page header |

Browser tests are outside `src` under the Playwright configuration. Add tests beside the closest ownership group and use shared render helpers when providers are required.

[Testing guide](../../docs/testing.md) · [Development guidelines](../../docs/development.md)
