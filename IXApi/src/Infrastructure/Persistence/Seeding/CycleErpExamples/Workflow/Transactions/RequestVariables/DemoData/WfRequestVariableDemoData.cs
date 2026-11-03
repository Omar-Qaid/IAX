using IAX.IXApi.Modules.Workflow.Variables;
using IAX.IXApi.Modules.Workflow.Requests;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Workflow;

internal static class WfRequestVariableDemoData
{
    internal static WfRequestVariable Create(long request, long variable, string? value) =>
        new WfRequestVariable { RequestId = request, VariableId = variable, VariableValue = value, DataAreaId = "dat", CreatedBy = "sys", OwnerAccountId = "sys" };
}
