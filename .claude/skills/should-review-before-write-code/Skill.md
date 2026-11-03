---
name: enterprise-aspnet-core
description: Write and refactor enterprise ASP.NET Core ERP code using organized modules, dedicated services, reusable logic, and existing architecture. Use for basic ERP implementation and D365 F&O functional alignment.
---

# Enterprise ASP.NET Core Development

## Review before implementation
- Inspect relevant code and understand the complete business flow before changing it.
- Follow existing architecture, naming conventions, folder structure, and dependency patterns.
- Identify existing services and shared functionality before creating new components.
- Read only files relevant to the task.

## Module organization
- Organize code by business module and responsibility.
- Follow existing folders for Entities, Configuration, Dtos, Interfaces, Services, Controllers, and Validation.
- Keep controllers thin, business logic in services, and input validation in validators.
- Enforce business invariants in services or domain components so non-HTTP callers cannot bypass them.
- Keep persistence configuration separate from business logic.

## Dedicated business services
- Use dedicated services for business processes, following existing project names.
- Examples include SalesOrderService, PurchaseOrderService, InventoryClosingService, and LedgerPostingService.
- Keep each service focused on a clear responsibility.
- Let process services coordinate shared services without duplicating their logic.
- Avoid circular dependencies and oversized services.

## Reuse and generic patterns
- Reuse existing components before introducing new ones.
- Follow existing generic patterns such as IBaseService<T>, BaseController<T, TDto>, and BaseValidator<TDto> where appropriate.
- Keep transaction-specific operations in dedicated services rather than forcing them into generic CRUD.
- Extract shared functionality only when behavior and business meaning are shared.
- Reuse inventory and accounting services across sales and purchasing.
- Maintain one authoritative implementation for each posting rule.
- Avoid duplicate code, unnecessary abstractions, and overengineering.

## Implementation standards
- Use interfaces and constructor dependency injection consistently with the project.
- Use DTOs according to existing API conventions.
- Use asynchronous I/O and propagate cancellation tokens where supported.
- Follow existing error handling and structured logging conventions.
- Keep dependencies explicit and methods readable.
- Preserve existing behavior outside the requested changes.

## Transactions and data integrity
- Define transaction boundaries around complete business operations.
- Keep related document, inventory, and accounting updates atomic where they share a database transaction.
- Prevent duplicate posting and invalid document status transitions.
- Follow existing concurrency controls and tenant/company boundaries.
- Preserve posted transaction history; use supported correction or reversal flows.
- Avoid partial commits and silently swallowed errors.

## Basic ERP scope
- Implement only the requested basic ERP functionality aligned with verified standard D365 F&O behavior.
- Exclude advanced and optional features unless explicitly requested.
- Do not invent entities, fields, enum mappings, or business rules.
- Verify uncertain behavior using authoritative documentation or available metadata.
- Reuse existing entities and logic whenever they satisfy the requirement.
- Flag missing evidence instead of guessing.

## Execution and validation
- Complete all authorized work within the requested scope.
- Continue through independent tasks when one task is blocked; report the blocker clearly.
- Keep changes focused and avoid unrelated refactoring.
- Perform a build check and focused validation of changed functionality.
- Avoid deep testing and broad test suites unless explicitly requested.
- Report checks accurately; do not claim unperformed validation.

## Completion report
- Summarize what changed and why.
- State build and focused validation results.
- List remaining gaps or blockers concisely.



---
name: basic-erp-aspnet-core
description: Write and refactor basic ERP functionality in ASP.NET Core using existing architecture, dedicated services, reusable logic, and verified standard D365 F&O behavior.
---

# Basic ERP Development

## Scope
- Implement only essential ERP functionality within the requested modules.
- Cover basic sales, purchasing, inventory, general ledger, customer/vendor payments, settlement, and basic taxes where requested.
- Include required setups, transaction statuses, partial processing, cancellations, returns, and reversals within scope.
- Preserve the related inventory and accounting effects.
- Exclude advanced features, WHS, production, manufacturing, and financial dimension derivation.

## Understand before changing
- Inspect existing entities, relationships, services, validations, and posting flows.
- Compare existing behavior with verified standard D365 F&O basic functionality.
- Identify missing or incorrect behavior using code evidence.
- Do not invent entities, fields, enum mappings, or business rules.
- Flag uncertain requirements instead of guessing.

## Code organization
- Follow existing architecture, naming conventions, and module structure.
- Keep controllers thin, business logic in services, and input validation in validators.
- Enforce business rules in services so all callers receive the same checks.
- Use dedicated services for sales, purchasing, inventory closing, and ledger posting when required.
- Keep services focused and dependencies explicit.
- Use interfaces and dependency injection consistently with the project.

## Reuse
- Reuse existing components before creating new ones.
- Share inventory and accounting logic across modules.
- Do not duplicate posting, balance calculation, or validation logic.
- Follow existing generic service, controller, and validator patterns where appropriate.
- Keep business operations outside generic CRUD when they require specific rules.
- Avoid unnecessary abstractions and overengineering.

## Data integrity
- Keep related document, inventory, and accounting updates within appropriate transaction boundaries.
- Prevent duplicate posting and invalid status transitions.
- Preserve tenant/company isolation and existing concurrency controls.
- Preserve posted history through supported corrections and reversals.
- Apply consistent error handling and structured logging.

## Execution
- Complete authorized basic ERP gaps in dependency order.
- Continue until the requested scope is complete without asking to proceed after each step.
- Report genuine blockers and continue independent work.
- Avoid unrelated changes and advanced enhancements.

## Validation and reporting
- Run a build check and focused checks of changed functionality.
- Do not perform deep testing or broad test suites.
- Summarize completed changes, validation results, and remaining gaps concisely.