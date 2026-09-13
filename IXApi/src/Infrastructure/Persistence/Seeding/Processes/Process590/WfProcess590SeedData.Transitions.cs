using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Transitions;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process590;

public sealed partial class WfProcess590SeedData
{
    private static async Task SeedTransitionsAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfTransitions.IgnoreQueryFilters()
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var toAdd = new List<WfTransition>();

        var transitions = new[]
        {
            new { RecId = 35L, ProcessId = 590L, ActivityId = (long?)6040L, VariableId = 3464L, OperatorId = (byte)5, Value = "لا", StepId = 0L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)1 },
            new { RecId = 43L, ProcessId = 590L, ActivityId = (long?)6036L, VariableId = 3462L, OperatorId = (byte)5, Value = "لا", StepId = 0L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)1 },
            new { RecId = 223L, ProcessId = 590L, ActivityId = (long?)6040L, VariableId = 3464L, OperatorId = (byte)5, Value = "اعادة لمدير المعرض", StepId = 15699L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)2 },
            new { RecId = 224L, ProcessId = 590L, ActivityId = (long?)6040L, VariableId = 3466L, OperatorId = (byte)5, Value = "لا", StepId = 15708L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)3 },
            new { RecId = 252L, ProcessId = 590L, ActivityId = (long?)6260L, VariableId = 3558L, OperatorId = (byte)5, Value = "اعادة لموظف الموارد البشرية", StepId = 15703L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)1 },
            new { RecId = 265L, ProcessId = 590L, ActivityId = (long?)6046L, VariableId = 3567L, OperatorId = (byte)5, Value = "إعادة إلى موظف الموارد في المملكة العربية السعودية", StepId = 15703L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)1 },
            new { RecId = 279L, ProcessId = 590L, ActivityId = (long?)6260L, VariableId = 3558L, OperatorId = (byte)5, Value = "اعادة لموظف الموارد البشرية دول الخليج", StepId = 15708L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)2 },
            new { RecId = 290L, ProcessId = 590L, ActivityId = (long?)6036L, VariableId = 3462L, OperatorId = (byte)5, Value = "نعم", StepId = 15699L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)2 },
            new { RecId = 341L, ProcessId = 590L, ActivityId = (long?)6046L, VariableId = 3567L, OperatorId = (byte)5, Value = "نعم", StepId = 15704L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)2 },
            new { RecId = 342L, ProcessId = 590L, ActivityId = (long?)6046L, VariableId = 3567L, OperatorId = (byte)5, Value = "لا", StepId = 15704L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)3 },
            new { RecId = 343L, ProcessId = 590L, ActivityId = (long?)6041L, VariableId = 3465L, OperatorId = (byte)5, Value = "نعم", StepId = 15704L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)1 },
            new { RecId = 344L, ProcessId = 590L, ActivityId = (long?)6041L, VariableId = 3465L, OperatorId = (byte)5, Value = "لا", StepId = 15704L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)2 },
            new { RecId = 354L, ProcessId = 590L, ActivityId = (long?)6260L, VariableId = 3558L, OperatorId = (byte)5, Value = "إعادة لمدير المعرض (خارج السعودية)", StepId = 15708L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)3 },
            new { RecId = 355L, ProcessId = 590L, ActivityId = (long?)6046L, VariableId = 3567L, OperatorId = (byte)5, Value = "اعادة لمدير المعرض في دول الخليج", StepId = 15708L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)2 },
            new { RecId = 444L, ProcessId = 590L, ActivityId = (long?)null, VariableId = 3466L, OperatorId = (byte)5, Value = "نعم", StepId = 15698L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)1 },
            new { RecId = 445L, ProcessId = 590L, ActivityId = (long?)null, VariableId = 3466L, OperatorId = (byte)5, Value = "لا", StepId = 15698L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)2 },
            new { RecId = 446L, ProcessId = 590L, ActivityId = (long?)6036L, VariableId = 3462L, OperatorId = (byte)5, Value = "12", StepId = 16071L, RequestControlId = (long?)null, IsActive = true, SortOrder = (byte)3 }
        };

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
            
            await db.Database.OpenConnectionAsync(ct);
            try
            {
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfTransitions ON", ct);
                await db.SaveChangesAsync(ct);
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfTransitions OFF", ct);
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        }
    }
}
