# IXApp frontend documentation

This directory documents the frontend implemented in `IXApp/src`. It is intended for developers changing the application, shared UI, page patterns, or business modules. The source code remains authoritative when documentation and implementation disagree.

## Start here

| Topic | Document |
| --- | --- |
| Architecture and dependency direction | [Architecture boundaries](ARCHITECTURE-BOUNDARIES.md) |
| Bootstrap, providers, theme, and app stores | [Application layer](app.md) |
| Routes, layouts, navigation, and shell behavior | [Routing and layouts](routing-and-layouts.md) and [App shell](app-shell.md) |
| API transport, server state, form state, and UI state | [API and state](api-and-state.md) |
| Authentication, sessions, and authorization | [Authentication](authentication.md) |
| Low-level infrastructure and error handling | [Core layer](core.md) |
| Shared component catalog | [Shared layer](shared.md) |
| Page-pattern catalog and implementation status | [Page patterns](patterns.md) |
| Business features and pages | [Business modules](modules.md) |
| Process Builder | [Process Builder](process-builder.md) |
| UI conventions, accessibility, RTL, and responsive behavior | [UI/UX and responsive design](ui-ux-and-responsive.md) |
| Coding, naming, and contribution workflow | [Development guidelines](development.md) |
| Unit, integration, and browser tests | [Testing](testing.md) |
| Mock data and mock adapters | [Mocks](mocks.md) |

## Source code map

Start with [`src/README.md`](../src/README.md), then follow the folder-level navigation:

| Area | Folder documentation |
| --- | --- |
| Application composition | [`src/app`](../src/app/README.md) |
| Assets | [`src/assets`](../src/assets/README.md) |
| Core infrastructure | [`src/core`](../src/core/README.md) |
| Mock datasets | [`src/mocks`](../src/mocks/README.md) |
| Business modules | [`src/modules`](../src/modules/README.md) |
| Page patterns | [`src/patterns`](../src/patterns/README.md) |
| Shared library | [`src/shared`](../src/shared/README.md) |
| Frontend tests | [`src/test`](../src/test/README.md) |

## Business modules

- [Administration](../src/modules/administration/README.md)
- [Dashboard](../src/modules/dashboard/README.md)
- [Accounts receivable](../src/modules/finance/accounts-receivable/README.md)
- [Finance foundation](../src/modules/finance/foundation/README.md)
- [Identity](../src/modules/identity/README.md)
- [Organization](../src/modules/organization/README.md)
- [Process Builder](../src/modules/process-builder/README.md)
- [Workflow](../src/modules/workflow/README.md)

## Shared UI guides

- [Shared documentation index](shared/README.md)
- [Action pane](shared/action-pane.md)
- [Data grid](shared/data-grid.md)
- [Dialogs](shared/dialogs.md)
- [FastTabs](shared/fast-tabs.md)
- [Feedback states](shared/feedback.md)
- [Fields](shared/fields.md)
- [Forms and validation](shared/forms.md)
- [Hooks](shared/hooks.md)
- [Logistics drawers](shared/logistics.md)
- [Lookups](shared/lookups.md)
- [Page primitives](shared/page.md)
- [Utilities](shared/utilities.md)

## Pattern guides

The canonical status table is in [Page patterns](patterns.md). The [pattern documentation index](patterns/README.md) connects compatibility guides to their source folders. Some folders in `src/patterns` are partial scaffolds; the docs label these as scaffolds and do not describe them as implemented.

## Documentation rules

- Use repository-relative Markdown links; never commit local `file:///` links.
- Reference real source names and public props.
- Distinguish implemented behavior, compatibility aliases, mock-only behavior, and empty scaffolds.
- Keep cross-cutting policy in one guide and link to it instead of copying it.
- Update documentation in the same change when routes, public component contracts, environment variables, or architectural boundaries change.
