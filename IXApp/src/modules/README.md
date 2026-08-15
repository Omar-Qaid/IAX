# Business modules

Modules own business pages, DTOs, endpoint wrappers, queries, validation, and feature-specific state. `index.ts` is the current top-level module export file.

| Module | Current responsibility | Documentation |
| --- | --- | --- |
| `administration` | Application/user settings and number sequences | [README](administration/README.md) |
| `dashboard` | Operations landing dashboard | [README](dashboard/README.md) |
| `finance/accounts-receivable` | Customers, groups, parameters, payment setup, and sales orders | [README](finance/accounts-receivable/README.md) |
| `finance/foundation` | Currencies, exchange rates, and exchange-rate types | [README](finance/foundation/README.md) |
| `identity` | Login page | [README](identity/README.md) |
| `organization` | Legal-entity maintenance | [README](organization/README.md) |
| `process-builder` | Integrated workflow designer/editor | [README](process-builder/README.md) |
| `workflow` | Workflow setup and process/activity/step/variable pages and APIs | [README](workflow/README.md) |

Modules are routed through `app/routes/pageRegistry.ts`; they do not own an independent router. Direct cross-module imports are rejected by the architecture audit, although Process Builder currently has documented imports from Workflow.

[Module catalog](../../docs/modules.md) · [Architecture boundaries](../../docs/ARCHITECTURE-BOUNDARIES.md)
