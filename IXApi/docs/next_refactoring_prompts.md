# Next-Stage Refactoring Prompts for IXApi

Here are the copy-pasteable prompts you can send me (or any AI assistant in a future session) to execute the next phase of enterprise-grade modular monolith refactoring:

---

### Option 1: Modular Project Assembly Partitioning (Highly Recommended)
This transitions folder-level modules into physically isolated projects (`.csproj`) to enforce boundaries at compilation time.

> **Copy & Send this prompt:**
> ```text
> Partition the IXApi folder modules into isolated project assemblies (.csproj):
> 1. Create a sub-folder structure `src/Modules/` and create separate class library projects:
>    - `IAX.IXApi.Modules.Identity`
>    - `IAX.IXApi.Modules.Organization`
>    - `IAX.IXApi.Modules.Workflow`
>    - `IAX.IXApi.Modules.Finance`
>    - `IAX.IXApi.Modules.Communication`
>    - `IAX.IXApi.Modules.Administration`
> 2. Move code files into their corresponding projects and configure references according to the whitelisted boundaries in ArchitectureComplianceTests.cs:
>    - All business modules depend on Identity (for permission checks) and Administration (for audit logs/sequences).
>    - Modules must not have circular dependencies.
> 3. Update the solution file (.sln) and main Web API host project reference.
> 4. Verify compilation compiles clean and all unit tests pass.
> ```

---

### Option 2: Split the Monolithic DbContext into Module DbContexts
This decouples the database schema definitions so each module owns its DB entities configuration.

> **Copy & Send this prompt:**
> ```text
> Split the fat monolithic ApplicationDbContext in IXApi into separate Module-specific DbContexts:
> 1. Define separate DbContext classes for each module (e.g. IdentityDbContext, OrganizationDbContext, FinanceDbContext, WorkflowDbContext) inheriting from a shared base context or DbContext.
> 2. Move model builder configuration maps (OnModelCreating configurations) from the monolithic db setup to their respective DbContexts.
> 3. Register the separate DbContexts in each module's composition root class (e.g., in FinanceModule.cs register FinanceDbContext).
> 4. Ensure transactional integrity remains intact when workflow operations span multiple module updates.
> 5. Verify the project builds cleanly and all baseline tests pass.
> ```

---

### Option 3: Decouple Modules using Event-Driven Communication
This eliminates compile-time couplings between modules (e.g., Workflow directly calling Communication notifications service) by replacing them with asynchronous in-memory domain events.

> **Copy & Send this prompt:**
> ```text
> Decouple cross-module service dependencies in IXApi using MediatR or a light event-driven broker:
> 1. Identify cross-module direct calls (e.g., Workflow calling Communication Notifications service, or Organization calling Finance addresses).
> 2. Create in-memory integration events (e.g. ActivityAssignedEvent) and replace direct service references with an event publisher dispatching events.
> 3. Create event handlers in the target modules (e.g. an handler in Communication module that listens to ActivityAssignedEvent and sends the notification).
> 4. Clean up the using statements and test compliance rules to confirm that modules are now decoupled from each other.
> 5. Verify compilation and test suite runs clean.
> ```
