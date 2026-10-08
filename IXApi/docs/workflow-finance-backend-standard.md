# Workflow and Finance backend architecture standard

## Scope and source of truth

This is a backend-only architecture for `IXApi`. The Process Maker backend is represented by the `Workflow/Processes` feature and its neighboring definition and execution features. `Finance/Foundation/Currency` is the concrete Finance folder and namespace reference: it groups configurations, controllers, DTOs, entities, interfaces, services, and validation by capability. The existing `IXApi/ARCHITECTURE.md` dependency rules remain authoritative. Process Builder UI state and client API adapters are outside this scope.

The current backend is a modular monolith: one host, one EF Core context and database, feature-first modules, shared HTTP and persistence infrastructure. Finance must follow the same responsibility boundaries as Workflow without copying every Workflow file into every Finance feature.

## Reference flow: Workflow process definition

| Responsibility | Current reference | Finance standard |
|---|---|---|
| Entity and EF mapping | `WfProcess`, `WfProcessConfiguration` | Entity and configuration owned by the Finance feature; retain existing entity classes and mappings during structural work. |
| HTTP contract | `WfProcessDto` | Feature-specific request/response DTOs; preserve existing JSON and routes. |
| Validation | `WfProcessDtoValidator` | FluentValidation for nontrivial input rules; domain/state checks in the application service. |
| Mapping | `WfProcessMapping` | Explicit Mapster mapping only where defaults or navigation handling require it. |
| API | `WfProcessController` | Thin controller with permission, company scope and rate policy appropriate to the endpoint. |
| Application logic | `IWfProcessService`, `WfProcessService` | Feature service for real business operations; use `BaseService<T>` for ordinary CRUD. |
| Registration | `WorkflowModule.AddWorkflowModule` | Register required feature services in `FinanceModule.AddFinanceModule`. |

The reference is a useful decomposition, but it is not a template to copy literally. `WfProcessService` handles a child collection and number sequence; simple Finance tables do not need equivalent code. The Process Maker execution path also includes versioned definitions, assignments, transactions and authorization that are specific to Workflow. Finance document posting needs its own explicit transactional operation and document rules.

## Recommended project structure

```text
IXApi/src/Modules/Finance/
  FinanceModule.cs
  Foundation/<Capability>/
  AccountsReceivable/<Capability>/
  AccountsPayable/<Capability>/
  GeneralLedger/<Capability>/
  Inventory/<Capability>/
    Controllers/        # HTTP and authorization only
    Dtos/               # API request/response contracts
    Validation/         # request shape and basic business validation
    Services/           # orchestration, invariants, transactions
    Interfaces/         # contracts at real injection boundaries
    Entities/           # persisted domain types
    Configuration/      # EF mappings
    Mappings/           # nontrivial DTO/entity mapping
```

Folders are optional. Keep a small feature flat when that is clearer. Currency's feature namespace is `IAX.IXApi.Modules.Finance.Foundation.Currency`; its entity classes retain the existing `Finance.Entities` namespace. Finance controllers use a feature service interface that extends `IBaseService<T>` for inherited CRUD operations; its implementation extends `BaseService<T>` and receives `IUnitOfWork`. Custom services may use the existing Finance data context abstraction when the operation needs provider-specific EF access. A separate repository or mapper is added only when it owns real behavior. `Shared` receives only stable, business-neutral contracts used across modules. Finance-specific contracts remain in Finance.

## Backend rules

1. Controllers parse requests, authorize, call a service, and return existing response contracts. They should not coordinate multi-entity writes or own business transactions.
2. Services own business invariants, state transitions, number-sequence use and transaction boundaries. A command that changes a document must be idempotent or reject repeated application explicitly; check company and document state before mutation.
3. Queries use bounded pagination and server-side projections. Keep read-only lookup endpoints separate from document commands when their responsibilities differ.
4. DTOs are the public API. Validate input before mutation, and map navigation collections explicitly. Do not expose EF graphs accidentally.
5. Apply company scope on all read and write paths, including child rows and background jobs. Authorization is checked at the operation boundary and cannot depend only on a controller attribute.
6. EF mapping, table names, keys and migrations are unchanged by a structural refactor. Introduce a schema migration only for a separately approved behavior change.
7. Register concrete services centrally in `FinanceModule`; preserve lifetimes and existing routes, permission resources, DTO JSON names and response envelopes.
8. Test business transitions and company isolation at service/API boundaries. A compile gate alone does not prove database behavior.

## Finance classification and rollout

The feature inventory shows that Finance already uses this pattern unevenly. The rollout should classify each feature before editing it:

| Group | Examples | Required work |
|---|---|---|
| CRUD master data | Price discount groups, delivery modes, currencies, payment terms | Keep `BaseController`/`BaseService` where sufficient; add validators or mapping only for real rules; verify DI and permission consistency. |
| Document commands | Sales orders, confirmations, invoices, packing slips, journals, inventory journals | Extract multi-entity writes from controllers into focused application services; make transaction and state ownership explicit. |
| Query/lookup | Sales order item/unit lookups, inventory dimensions, tax reference data | Keep bounded projections and company filters; introduce query services only for reused or substantial logic. |
| Shared business calculations | Exchange rates, pricing, tax, inventory demand | Use narrow service contracts and one owner for each calculation; avoid duplicate logic in controllers. |
| Persistence-only types | InventTrans origin links, invoice/settlement rows, posting configurations | Keep entity/configuration under owner feature; do not manufacture CRUD APIs for internal tables. |

### Checkpoints

1. **Contract baseline:** inventory every Finance endpoint, service registration, validator, mapping and EF entity; capture routes, permission resources and DTO shapes. Mark in-progress sales-order confirmation work separately.
2. **Pilot:** refactor one complex, stable Finance document operation through a thin controller and application service. Validate request/response compatibility, company isolation, transaction behavior and build.
3. **Accounts Receivable:** apply the pilot pattern to other document commands, followed by CRUD and lookup consistency checks.
4. **Foundation and Accounts Payable:** align actual service boundaries and DI; preserve simple CRUD features.
5. **General Ledger and Inventory:** migrate document commands and shared calculations; verify stock/ledger invariants and concurrency.
6. **Final gates:** architecture checks, focused service/API tests, module build, full API build, and live database checks where available. Report any gate that could not run separately.

Each checkpoint is independently reviewable. Before moving a feature, record its current HTTP contract and domain invariants. Do not change entity classes or EF mappings as part of this architecture rollout.

## Current observations

- `PriceDiscGroupController` uses `BaseController<PriceDiscGroup, PriceDiscGroupDto>` with `IPriceDiscGroupService`; the implementation extends `BaseService<PriceDiscGroup>` and receives `IUnitOfWork`.
- `SalesTableController` delegates sales-order commands to focused services. Its remaining read projections are candidates for a query-service pass; ordinary lookup queries may remain in the controller when they carry no business rule.
- `ExchangeRateService` is currently a thin `BaseService<ExchangeRate>` wrapper. Keep or remove that wrapper only after checking actual injection and behavior; there is no evidence for adding more abstraction.
- The current Finance inventory contains entity-only, API-only and full-service features. Missing file types do not by themselves indicate a defect.

## Acceptance criteria for implementation

For each migrated feature: existing routes, permission checks, DTO serialization, company behavior, persistence mappings and business results remain stable; transactional document operations have one service owner; DI resolves; focused tests exercise changed behavior; and build results are recorded. The module-wide rollout is complete only when all Finance feature groups have been classified and every confirmed boundary violation has been addressed.

## Sales unit flow in the available ERP schema

The sales-line application service uses the item Sales module to default its unit and base price, the Inventory module to identify the stock unit, and `UnitOfMeasureConversion` to calculate inventory quantity. It checks product-specific conversion before a global rule, supports a direct or inverse factor, and rejects missing or unsupported conversions. Sales quantity, inventory quantity, and price unit remain separate values. Inventory demand uses inventory quantity; posted `PriceDiscTable` prices are evaluated for the requested sales unit. A base price from a different unit is not silently converted.

The current project does not define `InventItemSalesSetup`, `PriceDiscParameters`, or sales-agreement entities. Their defaults, activation rules, and agreement priority are outside this implementation. The current price search retains the project's existing posted-agreement eligibility rules; it is not a complete D365 pricing engine. The line service rejects edits to processed lines, so this flow updates open lines only.
