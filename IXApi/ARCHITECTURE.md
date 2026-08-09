# IXApi architecture

## Overview

IXApi is an ASP.NET Core 9 modular monolith. It runs as one process, uses one SQL Server database, and exposes controllers from independently compiled business-module assemblies. This document describes the current repository, including its known architectural compromises.

## Repository structure

```text
IXApi/
|-- Program.cs                 # Composition root and HTTP pipeline
|-- IXApi.csproj               # Executable ASP.NET Core host
|-- IAX.slnx
|-- Api/                       # Host-level controllers, filters, middleware
|-- Bootstrap/                 # Host registration and pipeline extensions
|-- src/
|   |-- Shared/                # Stable domain and application building blocks
|   |-- Infrastructure/        # Persistence and technical implementations
|   `-- Modules/
|       |-- Identity/
|       |-- Organization/
|       |-- Workflow/
|       |-- Finance/
|       |-- Communication/
|       `-- Administration/
|-- Tests/
|-- docs/
`-- scripts/
```

`Api` and `Bootstrap` compile into the host. The host excludes `src/**/*.cs`; each directory below `src` is compiled by its own class-library project.

## Projects and ownership

| Project | Responsibility |
|---|---|
| `IAX.IXApi` | Hosting, configuration, common HTTP behavior, and composition |
| `IAX.IXApi.Shared` | Domain primitives and business-neutral application contracts |
| `IAX.IXApi.Infrastructure` | EF Core, repositories, seeding, caching, files, identity context, and SignalR |
| `IAX.IXApi.Modules.Identity` | Authentication, users, roles, permissions, and impersonation |
| `IAX.IXApi.Modules.Organization` | Companies, departments, employees, organization lookups, announcements, and attachments |
| `IAX.IXApi.Modules.Workflow` | Workflow definition, requests, execution, controls, and data exchange |
| `IAX.IXApi.Modules.Finance` | Finance foundation, receivables, payables, general ledger, and inventory |
| `IAX.IXApi.Modules.Communication` | Notifications and chat |
| `IAX.IXApi.Modules.Administration` | Audit logs, jobs, settings, number sequences, and data management |
| `IXApi.Tests` | Unit and architecture compliance tests |

## Business modules

### Identity

Owns JWT and external authentication, token blacklisting, ASP.NET Core Identity users and roles, permissions, and impersonation. The host configures Identity with the shared `ApplicationDbContext` and an authenticated fallback authorization policy.

### Organization

Owns companies, departments, employees, employee categories and groups, managers, management levels, nationalities, occupations, genders, showrooms, announcements, and attachments.

### Workflow

Owns processes, activities, requests, transitions, steps, variables, performers, priorities, controls, operators, assignments, transfers, validation, and Excel data exchange. It integrates with Administration jobs and Communication notifications.

### Finance

Owns the ERP/finance capability:

- `Foundation`: currency, dimensions, legal entities, logistics addresses, payments, delivery, markup, and tax.
- `AccountsReceivable`: customers, transactions, invoices, packing slips, sales, posting, and settlement.
- `AccountsPayable`: vendors and related setup.
- `GeneralLedger`: ledgers, accounts, fiscal calendars, journals, and banking.
- `Inventory`: products, dimensions, locations, journals, transactions, stock, reservations, counting, costing, transfers, and units.

The current assembly and registration names are `Finance` and `AddFinanceModule`. Older references to an `ERP` module or `AddErpModule` are obsolete.

### Communication

Owns chat and notifications. Notification senders include in-app, email, SMS, push, WhatsApp, Teams, Slack, and webhook channels. A hosted service processes scheduled notifications.

### Administration

Owns audit logs, background jobs, settings, number sequences, and import/export data management. A hosted processor executes registered background-job handlers.

## Feature organization

Organize by business capability first. Larger features may use these responsibility folders when needed:

```text
Feature/
|-- Controllers/
|-- Services/
|-- Repositories/
|-- Entities/
|-- Dtos/
|-- Validation/
|-- Mappings/
`-- Configuration/
```

The subfolders are optional. Do not add empty folders, marker interfaces, pass-through services, or repositories only to reproduce the template. Feature-specific controllers, DTOs, validators, mappings, configurations, and services remain with their feature. Move a type to Shared only when it is stable, business-neutral, and genuinely used by multiple modules.

## Runtime composition

`Program.cs` registers modules explicitly in this order:

```text
AddApplicationServices
AddCommunicationModule
AddAdministrationModule
AddWorkflowModule
AddFinanceModule
AddIdentityModule
AddOrganizationModule
```

Each module exposes one `Add*Module` entry point for its services, validators, handlers, and hosted services. Bootstrap owns application-wide infrastructure, authentication, authorization, CORS, rate limiting, caching, compression, OpenAPI, and controller configuration.

The pipeline includes exception handling, infrastructure middleware, development API documentation, HSTS outside development, CORS, WebSockets, authentication, authorization, rate limiting, output caching, and compression. It maps controllers, SignalR hubs at `/hubs/realtime` and `/hubs/chat`, and an anonymous `/health` endpoint. Database initialization runs after endpoint mapping.

## Persistence and company scope

Infrastructure contains one `ApplicationDbContext`, derived from `IdentityDbContext`, for every module. EF mappings are applied centrally; generic repositories and a unit of work are shared infrastructure. This is a shared-database modular monolith, not database-per-module or schema-per-module isolation.

`ApplicationDbContext.GetDataAreaId()` resolves the current company from:

1. `X-Company` header.
2. `X-DataAreaId` header.
3. The authenticated user's `Company` or `DataAreaId` claim.
4. Default value `dat`.

Consequently, changes to mappings, migrations, query filters, or company resolution require solution-wide integration validation.

## Current dependency graph

```text
Host -> Shared, Infrastructure, all Modules
Infrastructure -> Shared, all Modules
Identity -> Shared
Administration -> Shared, Identity
Organization -> Shared, Identity, Administration
Finance -> Shared, Identity, Organization, Administration
Communication -> Shared, Identity, Organization, Finance, Administration
Workflow -> Shared, Identity, Organization, Finance, Communication, Administration
Shared -> no project references
```

The graph is acyclic, but it creates a deep ordering between modules. Workflow and Communication know several other modules directly. Infrastructure references every module because its central DbContext, model building, and seeding use module entity types.

## Dependency rules

1. Shared must remain independent of the host, Infrastructure, and business modules.
2. Modules must not reference Api, Bootstrap, Infrastructure, or the executable host.
3. Treat module internals as private by convention; expose narrow provider-owned contracts or events for necessary collaboration.
4. Do not make new cross-module calls through controllers, repositories, another module's concrete service, or direct DbContext access.
5. Infrastructure may implement contracts owned by modules or Shared.
6. Module registration belongs in its `*Module.cs`; host-wide technical registration belongs in Bootstrap.
7. Preserve routes, DTO serialization, authorization, table names, and EF mappings during structural refactoring unless a versioned behavior change is approved.
8. Add abstractions only at a real boundary or when multiple implementations are required.

## Cross-cutting behavior

- FluentValidation provides request validation.
- Mapster provides object mapping.
- Global exception handling returns problem details with a trace identifier.
- ASP.NET Core Identity, JWT, and external providers provide authentication.
- Dynamic permission policies and an authenticated fallback policy provide authorization.
- OpenAPI metadata and Scalar UI provide development API documentation.
- SignalR provides realtime messaging.
- Administration and Communication provide hosted background processing.

## Known deviations and technical debt

- `Api` and `Bootstrap` remain at root while class libraries live below `src`; this is valid but visually inconsistent.
- Module project references form a deep dependency chain rather than narrow contract-based collaboration.
- Infrastructure references all modules, and the large central `ApplicationDbContext` imports their entity types. Persistence is not module-isolated.
- `Identity/Authentication/Authentication` duplicates a path segment.
- Organization mixes top-level features with `Features/OrgEmployeeCategory` and `Features/OrgEmployeeGroup`.
- Finance retains some ERP terminology in type names. Rename only after assessing API consumers and persistence mappings.
- Build-generated `bin` and `obj` folders exist below `src`; they are not architecture and must remain ignored.
- Shared covers many application concerns. Require a cross-module, business-neutral reason for every new shared type.
- The current worktree is a large structural move from former root-level `Modules`, `Shared`, and `Infrastructure` directories into `src`. Review delete/add pairs as moves before assuming code was lost.

## Change checklist

1. Confirm the owning business module.
2. Reject new dependency cycles and unnecessary project references.
3. Update namespaces, registration, EF discovery, and tests together.
4. Verify routes, contracts, authorization, tables, filters, and seeding remain compatible.
5. Run `dotnet build IAX.slnx` and `dotnet test Tests/IXApi.Tests.csproj`.
6. For persistence changes, verify startup, `/health`, authentication, company filtering, and database initialization in a safe environment.

## Validation status

The repository structure, startup composition, project references, and persistence model were re-audited on 2026-08-09. Build and test results must be refreshed after structural changes.
