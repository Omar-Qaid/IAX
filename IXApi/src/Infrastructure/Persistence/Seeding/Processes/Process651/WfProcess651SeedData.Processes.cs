using IAX.IXApi.Modules.Workflow.Processes;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process651;

public sealed partial class WfProcess651SeedData
{
    private static async Task SeedProcessesAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        if (await db.WfProcesses.IgnoreQueryFilters().AnyAsync(x => x.RecId == 651, ct))
            return;

        db.WfProcesses.Add(new WfProcess
        {
            RecId = 651,
            Code = "PROC651",
            Name = "Return or reverse a payment for the online store",
            NameAlias = "ارجاع أو عكس عملية دفع للمتجر الإلكتروني",
            Description = null,
            CategoryId = 16,
            IsActive = true,
            IsRepeatable = true,
            RepeatIntervalHours = 0,
            MandatoryDocuments = false,
            IsSystemDefined = false,
            PriorityId = 3,
            Score = 87.48m,
            ProcessTypeId = 1,
            CreatedBy = owner,
            OwnerAccountId = owner
        });

        await Chunks.WfProcessSeedData.SaveIdentityRowsAsync(db, "WfProcesses", ct);
    }
}
