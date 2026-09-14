using IAX.IXApi.Modules.Workflow.Transitions;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process603;

public sealed partial class WfProcess603SeedData
{
    private static async Task SeedTransitionsAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingTransitions = await db.WfTransitions
            .IgnoreQueryFilters()
            .Where(x => x.ProcessId == 603L)
            .ToListAsync(ct);

        var newTransitions = new List<WfTransition>
        {
            new() { RecId = 125, ProcessId = 603, ActivityId = 6140, VariableId = 3502, OperatorId = 5, Value = "نعم", StepId = 0, IsActive = true, SortOrder = 1 },
            new() { RecId = 243, ProcessId = 603, ActivityId = 6140, VariableId = 3502, OperatorId = 5, Value = "لا", StepId = 15793, IsActive = true, SortOrder = 2 },
            new() { RecId = 245, ProcessId = 603, ActivityId = null, VariableId = 3555, OperatorId = 5, Value = "لا", StepId = 15899, IsActive = true, SortOrder = 1 },
            new() { RecId = 291, ProcessId = 603, ActivityId = 6245, VariableId = 3554, OperatorId = 5, Value = "نعم", StepId = 0, IsActive = true, SortOrder = 1 },
            new() { RecId = 292, ProcessId = 603, ActivityId = 6245, VariableId = 3554, OperatorId = 5, Value = "اعادة لمحاسب مبيعات المملكة العربية السعودية", StepId = 15794, IsActive = true, SortOrder = 2 },
            new() { RecId = 298, ProcessId = 603, ActivityId = 6141, VariableId = 3503, OperatorId = 5, Value = "تحويل المعاملة لمحاسب السعودية", StepId = 15794, IsActive = true, SortOrder = 1 },
            new() { RecId = 299, ProcessId = 603, ActivityId = 6141, VariableId = 3503, OperatorId = 5, Value = "تحويل المعاملة لمحاسب دول الخليج", StepId = 15899, IsActive = true, SortOrder = 2 },
            new() { RecId = 300, ProcessId = 603, ActivityId = 6143, VariableId = 3577, OperatorId = 5, Value = "نعم", StepId = 0, IsActive = true, SortOrder = 1 },
            new() { RecId = 301, ProcessId = 603, ActivityId = 6143, VariableId = 3577, OperatorId = 5, Value = "لا", StepId = 0, IsActive = true, SortOrder = 2 },
        };

        foreach (var t in newTransitions)
        {
            if (!existingTransitions.Any(x => x.RecId == t.RecId))
            {
                t.CreatedBy = owner;
                t.OwnerAccountId = owner;
                db.WfTransitions.Add(t);
            }
        }

        await SaveWithIdentityAsync(db, "WfTransitions", ct);
    }
}
