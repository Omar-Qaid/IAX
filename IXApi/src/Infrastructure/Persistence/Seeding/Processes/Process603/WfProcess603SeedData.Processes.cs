using IAX.IXApi.Modules.Workflow.Processes;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process603;

public sealed partial class WfProcess603SeedData
{
    private static async Task SeedProcessesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        // Preserve existing process configuration, including soft-deleted records.
        if (await db.WfProcesses.IgnoreQueryFilters().AnyAsync(x => x.RecId == 603, ct))
            return;

        db.WfProcesses.Add(new WfProcess
        {
            RecId = 603,
            Code = "PROC603",
            Name = "Daily Deposit",
            NameAlias = "الايداع اليومي",
            Description = null,
            CategoryId = 7, // Placeholder category, adjust if needed
            IsActive = true,
            IsRepeatable = true,
            RepeatIntervalHours = 0,
            MandatoryDocuments = false,
            IsSystemDefined = false,
            PriorityId = 3,
            Score = 100m,
            ProcessTypeId = 1,
            CreatedBy = owner,
            OwnerAccountId = owner
        });

        await Chunks.WfProcessSeedData.SaveIdentityRowsAsync(db, "WfProcesses", ct);
    }
}
