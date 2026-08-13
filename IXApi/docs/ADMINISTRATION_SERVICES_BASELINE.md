# Administration Services Compatibility Baseline

This document freezes the externally observable Administration behavior before
the shared-contract refactor. Refactoring must preserve these routes, DTOs,
permissions, persistence tables, and business rules unless a separate API
version or migration is explicitly approved.

## Capability ownership

| Capability | Current public service | Current implementation owner |
| --- | --- | --- |
| Audit queries | `ISysAuditLogService` | Administration/AuditLogs |
| Audit value policy | `ISysAuditService` | Administration/AuditLogs |
| Background jobs | `ISysBackgroundJobManager` | Administration/BackgroundJobs |
| Job handlers | `ISysBackgroundJobHandler` | Administration contract, module implementations |
| Data import/export | `ISysDataManagementService` | Administration/DataManagement |
| Excel serialization | `ISysExcelService` | Administration/DataManagement |
| Number generation | `ISysNumberSequenceService` | Administration/NumberSequences |
| Global/user settings | `ISysSettingsService` | Administration/Settings |

## API compatibility surface

- `api/v1/SysAuditLog`, including `GET by-record` and inherited CRUD routes.
- `api/v1/SysBackgroundJob`, including list, detail, executions, dashboard,
  handlers, create, schedule update, trigger, pause, resume, cancel, and delete.
- `api/v1/SysDataManagement/{entityName}`, including import, both export forms,
  template, allowed entities, and fields.
- `api/v1/SysNumberSequence`, including inherited CRUD, next, peek, and reset.
- `api/v1/SysSettings/global` and `api/v1/SysSettings/user` GET/PUT routes.

All existing controllers remain authenticated. Existing `DomainPermission`
keys on audit logs and number sequences remain unchanged.

## Persistence compatibility

The refactor must not rename Administration tables, columns, indexes, foreign
keys, query filters, or change number allocation transaction semantics. It must
not generate a migration solely for moving contracts or implementations.

## Identified extraction boundaries

Phase 2 may introduce narrow shared contracts for auditing, background-job
registration/scheduling, data exchange, number generation, and settings reads.
Existing `ISys*` interfaces remain as compatibility adapters until every module
consumer has migrated and the full solution passes validation.

Implementation-specific EF Core types remain inside Administration or
Infrastructure. Other modules must not acquire new references to Administration
entities, controllers, persistence contexts, or implementation namespaces.

## Required validation after each phase

1. Independent builds for Shared, Administration, and every affected module.
2. Full Release solution build and test suite.
3. Controller route, authorization, permission, and DTO compatibility tests.
4. EF Core pending-model-change check.
5. Startup and `/health` verification when database configuration is available.
