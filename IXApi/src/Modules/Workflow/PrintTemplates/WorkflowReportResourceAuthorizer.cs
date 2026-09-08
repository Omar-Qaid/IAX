using IAX.IXApi.Modules.Workflow.Persistence;
using IAX.IXApi.Modules.Workflow.Requests;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Workflow.PrintTemplates;

public sealed class WorkflowReportResourceAuthorizer(
    IWorkflowDataContext context,
    IWfRequestService requests) : IReportResourceAuthorizer
{
    public const int ProcessTableId = 476793887;
    public const int RequestTableId = 1001;

    public async Task<bool> CanReadAsync(int refTableId, long refRecId, CancellationToken cancellationToken = default) =>
        refTableId switch
        {
            ProcessTableId => await context.WfProcesses.AsNoTracking()
                .AnyAsync(item => item.RecId == refRecId && item.IsActive, cancellationToken),
            RequestTableId => await requests.CanAccessRequestAsync(refRecId, cancellationToken),
            _ => false
        };

    public async Task<bool> CanDesignAsync(int refTableId, long refRecId, CancellationToken cancellationToken = default) =>
        refTableId == ProcessTableId && await context.WfProcesses.AsNoTracking()
            .AnyAsync(item => item.RecId == refRecId && item.IsActive, cancellationToken);

    public async Task<string> GetDisplayNameAsync(int refTableId, long refRecId, CancellationToken cancellationToken = default)
    {
        if (refTableId != ProcessTableId) return string.Empty;
        return await context.WfProcesses.AsNoTracking()
            .Where(item => item.RecId == refRecId)
            .Select(item => item.Name ?? item.Code ?? string.Empty)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;
    }
}
