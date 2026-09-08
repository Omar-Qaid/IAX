using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Workflow.Execution;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Workflow.Requests;

public partial class WfRequestService
{
    public async Task<bool> CanAccessRequestAsync(long requestId, CancellationToken cancellationToken = default)
    {
        if (await CanViewAllRequestsAsync(cancellationToken)) return true;

        var userId = _currentUser.GetCurrentUserId();
        var employeeId = await GetCurrentEmployeeIdAsync(userId, cancellationToken);
        return await _context.WfRequests.AsNoTracking()
            .Where(item => item.RecId == requestId)
            .AnyAsync(item => item.CreatedBy == userId
                || employeeId.HasValue && item.EmployeeId == employeeId.Value
                || employeeId.HasValue && _context.Set<WfAssignment>()
                    .Any(assignment => assignment.RequestId == item.RecId && assignment.UserId == employeeId.Value),
                cancellationToken);
    }

    private async Task<bool> CanViewAllRequestsAsync(CancellationToken cancellationToken)
    {
        var permissions = await _permissions.GetPermissionKeysByUserAsync(
            _currentUser.GetCurrentUserId(), cancellationToken);
        return permissions.Contains("*") || permissions.Contains("Workflow.Requests.View");
    }

    private Task<long?> GetCurrentEmployeeIdAsync(string userId, CancellationToken cancellationToken) =>
        _context.Set<HcmWorker>().AsNoTracking()
            .Where(worker => worker.UserId == userId && worker.IsActive && !worker.IsDeleted)
            .Select(worker => (long?)worker.RecId)
            .FirstOrDefaultAsync(cancellationToken);
}
