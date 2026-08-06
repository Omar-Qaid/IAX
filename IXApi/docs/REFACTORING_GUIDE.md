# Code Quality Review & Refactoring Roadmap

This guide documents identified code smells, technical debt, and outlines a recommended modular reorganization plan.

---

## 1. Architectural Review & Identified Issues

During our deep analysis of the codebase, we highlighted several code smells and debt:

### Code Smells & Violations
1.  **Direct Module Cross-Coupling (High Coupling)**:
    *   *Issue*: While `ARCHITECTURE.md` states cross-module database relationships are supported, several classes depend directly on entities of other modules (e.g. `Workflow` referencing `Finance` and `Communication` directly).
    *   *Improvement*: Wrap cross-module communication inside interface contracts located in a module's `Abstraction/` folder (similar to how `Identity` uses `OrgEntity`).
2.  **Unused Import Pollution**:
    *   *Issue*: Base files such as `Entity.cs` contained using statements referencing arbitrary modules (`Finance`, `Organization`), causing artificial compilation coupling.
    *   *Improvement*: Solved during refactoring. Remove all unused imports.
3.  **Fat DbContext**:
    *   *Issue*: `ApplicationDbContext` is shared among all modules. It contains the dbsets for every module. This creates database schema coupling.
    *   *Improvement*: Split database contexts per module (e.g. `FinanceDbContext`, `WorkflowDbContext`) sharing the same database connection string, or use EF Core schema mapping.

---

## 2. Reorganization Plan

To transition `IXApi` to a clean, enterprise-grade architecture, we recommend the following folder reorganization layout:

```
src/
├── IAX.IXApi.Api/                  # Host & Web API (Controllers, Middleware, Hubs)
├── IAX.IXApi.Bootstrap/            # Program, DependencyInjection, Extensions
├── IAX.IXApi.Infrastructure/       # Core Persistence, Cache, local storage implementation
├── IAX.IXApi.Shared/               # Base primitives, entities, contracts, DTO envelopes
└── IAX.IXApi.Modules/              # Features (each module compiles to its own assembly project)
    ├── IAX.IXApi.Modules.Identity/
    ├── IAX.IXApi.Modules.Organization/
    ├── IAX.IXApi.Modules.Workflow/
    ├── IAX.IXApi.Modules.Finance/
    ├── IAX.IXApi.Modules.Communication/
    └── IAX.IXApi.Modules.Administration/
```

### Benefits
*   **Compile-Time Boundary Checks**: By separating modules into their own projects (`.csproj`), we can enforce modular boundaries at the compiler level. `NetArchTest` checks are still run, but compiler references will physically block illegal cross-module dependencies.
*   **Parallel Development**: Development teams can work on individual module projects without fear of merge conflicts or build breaks on unrelated modules.
*   **Easier Testing**: Modules can have isolated unit and integration test assemblies.
