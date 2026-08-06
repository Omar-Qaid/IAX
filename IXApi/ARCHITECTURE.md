# IAX IXApi architecture

IXApi is a single-deployment modular monolith. Business behavior is owned by modules; technical implementations are owned by Infrastructure; reusable primitives live in Shared.

## Top-level ownership

- `Modules/Identity`: authentication, users, roles, permissions, and impersonation.
- `Modules/Organization`: companies, employees, departments, organizational lookups, announcements, and attachments.
- `Modules/Workflow`: workflow definition, execution, requests, activities, performers, and workflow data exchange.
- `Modules/ERP`: ERP foundation, accounts receivable, accounts payable, general ledger, and inventory.
- `Modules/Communication`: notifications and chat.
- `Modules/Administration`: audit logs, background jobs, settings, number sequences, and data management.
- `Shared`: domain primitives and application contracts without hosting or persistence concerns.
- `Infrastructure`: EF Core persistence, repositories, seeding, caching, files, identity context, and realtime implementations.
- `Api`: HTTP middleware, filters, and common controller behavior.
- `Bootstrap`: application composition and ASP.NET Core registration extensions.

## Feature layout

Feature folders use the following names only when the responsibility exists:

```text
Feature/
├── Controllers/
├── Services/
├── Repositories/
├── Entities/
├── Dtos/
├── Validation/
├── Mappings/
└── Configuration/
```

Do not create empty folders or one-line pass-through abstractions solely to satisfy this shape.

## Dependency rules

1. `Shared` contains stable primitives and must not depend on API hosting or persistence implementations.
2. Module business code must not depend on `Bootstrap`.
3. Infrastructure may implement contracts owned by modules or Shared.
4. Module registration belongs to its `*Module.cs` composition entry point.
5. Cross-module database relationships currently remain supported for compatibility; new cross-module behavior should use a narrow contract owned by the providing module.
6. HTTP routes, DTO contracts, database table names, and EF mappings must remain stable during structural refactors.

## Composition

`Program.cs` registers application services, then each module explicitly:

```text
AddApplicationServices
AddCommunicationModule
AddAdministrationModule
AddWorkflowModule
AddErpModule
AddIdentityModule
AddOrganizationModule
```

Attribute registration remains as a temporary compatibility mechanism for existing services. New services should be registered by their owning module.
