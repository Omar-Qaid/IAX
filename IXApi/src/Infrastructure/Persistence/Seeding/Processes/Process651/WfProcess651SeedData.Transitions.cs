using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Transitions;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process651;

public sealed partial class WfProcess651SeedData
{
    private static async Task SeedTransitionsAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfTransitions.IgnoreQueryFilters()
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var transitions = new[]
        {
            new { RecId = 395L, ProcessId = 651L, ActivityId = (long?)6376L, VariableId = 3616L, OperatorId = (byte)5, Value = "نعم", StepId = 0L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)1 },
            new { RecId = 397L, ProcessId = 651L, ActivityId = (long?)6375L, VariableId = 3617L, OperatorId = (byte)5, Value = "لا", StepId = 16045L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)1 },
            new { RecId = 511L, ProcessId = 651L, ActivityId = (long?)6377L, VariableId = 3630L, OperatorId = (byte)5, Value = "اعادة لمقدم الطلب", StepId = 16045L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)1 },
            new { RecId = 564L, ProcessId = 651L, ActivityId = (long?)null, VariableId = 3618L, OperatorId = (byte)5, Value = "1", StepId = 16037L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)1 },
            new { RecId = 573L, ProcessId = 651L, ActivityId = (long?)6664L, VariableId = 3630L, OperatorId = (byte)5, Value = "NO", StepId = 16037L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)1 },
            new { RecId = 582L, ProcessId = 651L, ActivityId = (long?)6675L, VariableId = 3630L, OperatorId = (byte)5, Value = "NO", StepId = 16293L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)1 },
            new { RecId = 424L, ProcessId = 651L, ActivityId = (long?)6376L, VariableId = 3616L, OperatorId = (byte)5, Value = "لا", StepId = 16045L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)2 },
            new { RecId = 512L, ProcessId = 651L, ActivityId = (long?)6377L, VariableId = 3630L, OperatorId = (byte)5, Value = "اعادة لمحاسب المالية", StepId = 16038L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)2 },
            new { RecId = 565L, ProcessId = 651L, ActivityId = (long?)null, VariableId = 3618L, OperatorId = (byte)5, Value = "2", StepId = 16037L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)2 },
            new { RecId = 566L, ProcessId = 651L, ActivityId = (long?)null, VariableId = 3618L, OperatorId = (byte)5, Value = "3", StepId = 16037L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)3 },
            new { RecId = 567L, ProcessId = 651L, ActivityId = (long?)null, VariableId = 3618L, OperatorId = (byte)5, Value = "4", StepId = 16037L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)4 },
            new { RecId = 568L, ProcessId = 651L, ActivityId = (long?)null, VariableId = 3618L, OperatorId = (byte)5, Value = "5", StepId = 16037L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)5 },
            new { RecId = 569L, ProcessId = 651L, ActivityId = (long?)null, VariableId = 3618L, OperatorId = (byte)5, Value = "6", StepId = 16037L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)6 }
        };

        var toAdd = new List<WfTransition>();
        foreach (var item in transitions)
        {
            if (!existingIds.Contains(item.RecId))
            {
                toAdd.Add(new WfTransition
                {
                    RecId = item.RecId,
                    ProcessId = item.ProcessId,
                    ActivityId = item.ActivityId,
                    VariableId = item.VariableId,
                    OperatorId = item.OperatorId,
                    Value = item.Value,
                    StepId = item.StepId,
                    RequestControlId = item.RequestControlId,
                    SortOrder = item.SortOrder,
                    CreatedBy = owner,
                    OwnerAccountId = owner,
                    IsActive = item.IsActive
                });
            }
        }

        if (toAdd.Any())
        {
            await db.WfTransitions.AddRangeAsync(toAdd, ct);
            await SaveWithIdentityAsync(db, "WfTransitions", ct);
        }
    }
}
