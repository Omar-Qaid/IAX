---
name: basic-erp-refactoring
description: Refactor existing ASP.NET Core basic ERP code into organized modules and reusable services while preserving business behavior and data.
---

# Basic ERP Refactoring

## Understand existing code
- Inspect relevant entities, services, controllers, validators, and posting flows.
- Map dependencies and duplicated logic before making changes.
- Follow existing architecture, naming conventions, and folder structure.

## Refactoring rules
- Preserve business behavior, calculations, posting results, transaction statuses, and existing data.
- Preserve public API contracts unless explicitly authorized to change them.
- Keep controllers thin, business logic in services, and input validation in validators.
- Keep business invariants enforced within services.
- Organize code by module and responsibility.
- Separate sales, purchasing, inventory closing, and ledger posting responsibilities where applicable.
- Reuse shared inventory and accounting services instead of duplicating logic.
- Follow existing generic service, controller, and validator patterns where appropriate.
- Keep process-specific operations outside generic CRUD.
- Use interfaces and dependency injection consistently.
- Avoid circular dependencies, oversized services, unnecessary abstractions, and overengineering.

## Scope and integrity
- Refactor basic ERP functionality only.
- Do not introduce new features, entities, fields, or business rules.
- Record functional gaps separately; do not silently change behavior to match D365 F&O.
- Preserve transaction boundaries, concurrency controls, tenant/company isolation, and duplicate-posting protections.
- Follow existing error handling and structured logging conventions.
- Remove obsolete code only after verifying that it is no longer used.

## Execution and verification
- Make focused changes in dependency order.
- Complete the authorized scope without stopping after each service.
- Run a build check and focused checks of affected functionality.
- Avoid deep testing and broad test suites.
- Summarize refactored components, verification results, and remaining issues concisely.



---
name: basic-erp-data-integrity
description: Review and improve data integrity in existing ASP.NET Core basic ERP modules, including seed data, relationships, transactions, and posting consistency.
---

# Basic ERP Data Integrity

## Review
- Inspect existing entities, relationships, constraints, validations, and transaction flows.
- Identify orphan records, duplicates, invalid references, and inconsistent statuses.
- Report findings with affected components and supporting evidence.

## Integrity rules
- Enforce required fields, valid foreign keys, and appropriate uniqueness constraints.
- Scope uniqueness and references correctly by tenant/company.
- Use suitable decimal precision for quantities, prices, taxes, and amounts.
- Keep document totals consistent with lines, discounts, taxes, and rounding.
- Preserve balanced accounting entries and consistent inventory movements.
- Prevent duplicate posting, invalid status transitions, and concurrent conflicting updates.
- Keep related changes atomic using appropriate transaction boundaries.
- Protect posted records from direct modification or deletion; use supported corrections and reversals.
- Enforce business rules in services and structural constraints in the database.

## Seed data
- Preserve stable IDs, codes, relationships, and tenant/company scope.
- Seed dependencies before dependent records.
- Make reruns idempotent without overwriting user-maintained data.
- Keep seed data and completion tracking consistent.

## Execution
- Follow existing architecture and verified basic ERP business rules.
- Do not invent entities, relationships, or accounting behavior.
- Do not silently delete, merge, or rewrite inconsistent historical data.
- Report existing data conflicts before applying constraints that would fail.
- Keep fixes focused and reuse existing validation and services.
- Run a build check and focused integrity checks; avoid deep testing.
- Summarize fixes, verification results, and unresolved data conflicts.



---
name: basic-erp-structure-refactoring
description: Review, refactor, and lightly test ASP.NET Core basic ERP code to improve module structure, service organization, reuse, and maintainability.
---

# Basic ERP Structure Refactoring

## Review
- Inspect the existing solution structure, module dependencies, services, and tests.
- Identify misplaced code, duplicated logic, oversized classes, and circular dependencies.
- Follow existing architecture and naming conventions.

## Organize and refactor
- Organize code by business module and responsibility.
- Follow existing folders for Entities, Configuration, Dtos, Interfaces, Services, Controllers, and Validation.
- Keep controllers thin, business logic in services, and input validation in validators.
- Keep business invariants enforced within services.
- Separate sales, purchasing, inventory, and accounting responsibilities.
- Use dedicated process services where needed, such as inventory closing and ledger posting.
- Reuse shared inventory and accounting logic across modules.
- Follow existing generic patterns where appropriate; keep business operations outside generic CRUD.
- Use clear interfaces and dependency injection.
- Remove duplication and unused code after verifying references.
- Avoid unnecessary layers, abstractions, and unrelated changes.

## Preserve behavior
- Preserve business rules, calculations, posting results, API contracts, and existing data.
- Preserve transaction boundaries, concurrency controls, and tenant/company isolation.
- Do not introduce new ERP features, entities, or business rules.
- Report functional gaps separately from structural refactoring.

## Organize and verify tests
- Organize existing tests by module, service, and operation.
- Reuse fixtures and setup helpers without sharing mutable state.
- Preserve meaningful business assertions.
- Build affected projects and run focused tests for changed components.
- Check directly affected calculations, validations, and posting flows.
- Use an isolated database for tests that write data.
- Avoid deep testing and broad test suites.

## Completion
- Complete the authorized scope in dependency order.
- Summarize structural changes, removed duplication, verification results, and remaining issues.