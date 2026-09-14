using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process603;

public sealed partial class WfProcess603SeedData
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {

       
        await EnsureActivityTypeAsync(db, owner, ct);
         /*
          -- 1. Process
          -- 2. Process Users
          -- 3. Process Variable Values
          -- 4. Process Request Controls
          -- 5. Process Request Controls Mapping Variables
          -- 6. Process Request Controls Options and Validations
          -- 7. Process Request Controls Transitions
          -- 8. Process Steps   
          -- 9. Process Activities
          -- 10. Process Activity Controls
          -- 11. Process Activity Controls Mapping Variables
          -- 12. Process Activity Controls Options and Validations
          -- 13. Process Activity Controls Transitions
        */
        await SeedProcessesAsync(db, owner, ct);
        await SeedStepsAsync(db, owner, ct);
        await SeedActivitiesAsync(db, owner, ct);
        await SeedActivityControlsAsync(db, owner, ct);
        await SeedRequestControlsAsync(db, owner, ct);
        await SeedVariablesAsync(db, owner, ct);
        await SeedTransitionsAsync(db, owner, ct);
        await SeedUsersProcessesAsync(db, owner, ct);
        await SeedRequestMappingVariablesAsync(db, owner, ct);
        await SeedActivityMappingVariablesAsync(db, owner, ct);
        await SeedOptionsAndValidationsAsync(db, owner, ct);


          /*
        -- 1. Request Header
        -- 2. Request Variables
        -- 3. Process Variables Values
        -- 4. Request Control -> Variable Mapping
        -- 5. Request Details
        -- 6. Workflow Assignments
        -- 7. Workflow Process Data / Tasks
        -- 8. Activity Details
        -- 9. Activity Control -> Variable Mapping
        */
        await SeedRequestsAsync(db, owner, ct);
        await SeedPrintTemplateAsync(db, owner, ct);
        // Add Performers seeding here when available
    }

    private static async Task EnsureActivityTypeAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        if (await db.WfActivityTypes.IgnoreQueryFilters().AnyAsync(x => x.RecId == 2, ct))
            return;

        db.WfActivityTypes.Add(new IAX.IXApi.Modules.Workflow.Activities.WfActivityType
        {
            RecId = 2,
            Code = "NORMAL",
            Name = "مرحلة عادية",
            Description = "مرحلة عادية",
            SortOrder = 2,
            IsActive = true,
            CreatedBy = owner,
            OwnerAccountId = owner
        });
        await SaveWithIdentityAsync(db, "WfActivityTypes", ct);
    }

    private static async Task SaveWithIdentityAsync(ApplicationDbContext db, string table, CancellationToken ct)
    {
        if (!db.ChangeTracker.HasChanges()) return;

        await db.Database.OpenConnectionAsync(ct);
        try
        {
            await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(db.Database, $"SET IDENTITY_INSERT {table} ON", ct);
            await db.SaveChangesAsync(ct);
            await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(db.Database, $"SET IDENTITY_INSERT {table} OFF", ct);
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}
