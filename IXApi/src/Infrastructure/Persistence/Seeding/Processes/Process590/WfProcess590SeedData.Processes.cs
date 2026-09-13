using IAX.IXApi.Modules.Workflow.Processes;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process590;

public sealed partial class WfProcess590SeedData
{
    private static async Task SeedProcessesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        // Preserve existing process configuration, including soft-deleted records.
        if (await db.WfProcesses.IgnoreQueryFilters().AnyAsync(x => x.RecId == 590, ct))
            return;

        db.WfProcesses.Add(new WfProcess
        {
            RecId = 590,
            Code = "PROC590",
            Name = "Disciplinary Action Salesman",
            NameAlias = "رصد مخالفة بائع",
            Description = null,
            CategoryId = 7,
            IsActive = true,
            IsRepeatable = true,
            RepeatIntervalHours = 0,
            MandatoryDocuments = false,
            IsSystemDefined = false,
            PriorityId = 3,
            Score = 90.39m,
            ProcessTypeId = 1,
            CreatedBy = owner,
            OwnerAccountId = owner,
            // Legacy AllowAttachments and CanSaveRequest have no current model fields.
        });

        await Chunks.WfProcessSeedData.SaveIdentityRowsAsync(db, "WfProcesses", ct);
    }
}
