# Development & Coding Standards

This manual provides instructions for developers writing new features, registering services, and adhering to codebase patterns.

---

## 1. Naming Conventions

All code must adhere to standard Microsoft .NET C# conventions:

| Element | Rule | Example |
| :--- | :--- | :--- |
| **Classes / Interfaces** | PascalCase. Interfaces prefixed with `I`. | `HcmWorker`, `IHcmWorkerService` |
| **Methods / Properties** | PascalCase. | `GetWorkerByIdAsync`, `CreatedBy` |
| **Private Fields** | camelCase with leading underscore. | `_dbContext`, `_cache` |
| **Local Variables** | camelCase. | `pageNumber`, `totalCount` |
| **Database Entities** | Singular PascalCase. | `SysNotification`, `WfProcess` |
| **DTO classes** | Suffix with `Dto`. | `AspNetUserDto` |
| **DTO Validators** | Suffix with `DtoValidator`. | `InventTableDtoValidator` |

---

## 2. Dependency Injection Registration

### The Explicit Registry Rule
Following clean-architecture practices, we **do not** use reflection-based dynamic assemblies scanning. All services must be explicitly registered inside their respective module composition root file (e.g. `FinanceModule.cs`).

*   **Transient Services**: Use for lightweight, stateless services.
    ```csharp
    services.AddTransient<IMyTransientService, MyTransientService>();
    ```
*   **Scoped Services**: Use for services that hold state or depend on DbContext (which is scoped).
    ```csharp
    services.AddScoped<IMyScopedService, MyScopedService>();
    ```
*   **Singleton Services**: Use for stateless utilities or caches.
    ```csharp
    services.AddSingleton<IMySingletonService, MySingletonService>();
    ```

---

## 3. Caching Engine (`LookupCacheService`)

For frequently queried lookup data (such as address lists, tax codes, currencies), use `ILookupCacheService`:

```csharp
public interface ILookupCacheService
{
    Task<T?> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> factory, TimeSpan? expiration = null);
    void Remove(string cacheKey);
}
```

### Usage Pattern
```csharp
public async Task<IEnumerable<CurrencyDto>> GetActiveCurrenciesAsync()
{
    const string CacheKey = "active_currencies";
    return await _cacheService.GetOrSetAsync(
        CacheKey,
        async () => await FetchCurrenciesFromDbAsync(),
        TimeSpan.FromHours(1)
    );
}
```

---

## 4. Background Job System

`IXApi` hosts a background job execution engine inside the `Administration/BackgroundJobs/` sub-folder:

### Implementing a Background Job Handler
Create a class implementing `ISysBackgroundJobHandler` and specify a unique `JobKey`:

```csharp
public class MyCustomJobHandler : ISysBackgroundJobHandler
{
    public string JobKey => "CustomJobExecution";

    public async Task ExecuteAsync(SysBackgroundJobContext context, CancellationToken cancellationToken)
    {
        // Resolve scoped dependencies from context.Services
        var dbContext = context.Services.GetRequiredService<ApplicationDbContext>();
        
        // Execute business logic...
        context.Output = "Job completed successfully.";
    }
}
```
Register the handler in `AdministrationModule.cs` using `services.AddScoped<ISysBackgroundJobHandler, MyCustomJobHandler>();`.
