using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Execution;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Workflow.Requests;

public partial class WfRequestService
{
    public async Task SaveMailActivityControlsAsync(
        long requestId,
        SaveMailActivityControlsDto submission,
        CancellationToken cancellationToken = default)
    {
        if (submission.AssignmentId <= 0)
            throw new InvalidOperationException("A current workflow assignment is required.");

        var request = await _context.WfRequests
            .FirstOrDefaultAsync(item => item.RecId == requestId, cancellationToken)
            ?? throw new KeyNotFoundException("Workflow request not found.");
        if (request.IsFinished || request.IsStopped || !request.IsActive)
            throw new InvalidOperationException("Activity controls cannot be changed after the request is closed.");

        var assignment = await _context.Set<WfAssignment>()
            .FirstOrDefaultAsync(item => item.RecId == submission.AssignmentId &&
                item.RequestId == requestId, cancellationToken)
            ?? throw new KeyNotFoundException("Workflow assignment not found.");
        if (assignment.IsFinished)
            throw new InvalidOperationException("This workflow activity is already completed.");

        var employeeId = await GetCurrentEmployeeIdAsync(
            _currentUser.GetCurrentUserId(), cancellationToken);
        if (!employeeId.HasValue || assignment.UserId != employeeId.Value)
            throw new UnauthorizedAccessException("Only the employee responsible for this activity can enter its values.");

        var controls = await _context.Set<WfActivityControl>()
            .Where(item => item.ActivityId == assignment.ActivityId && item.IsActive)
            .ToDictionaryAsync(item => item.RecId, cancellationToken);
        var submittedValues = submission.Values
            .GroupBy(item => item.ActivityControlId)
            .ToDictionary(group => group.Key, group => group.Last().Value ?? string.Empty);
        if (submittedValues.Keys.Any(id => !controls.ContainsKey(id)))
            throw new InvalidOperationException("One or more controls do not belong to the current activity.");
        if (submittedValues.Values.Any(value => value.Length > 255))
            throw new InvalidOperationException("An activity-control value cannot exceed 255 characters.");

        var controlIds = submittedValues.Keys.ToList();
        var existing = await _context.WfActivityDetails
            .Where(item => item.AssignmentID == assignment.RecId && controlIds.Contains(item.ControlDataId))
            .ToListAsync(cancellationToken);

        foreach (var (controlId, value) in submittedValues)
        {
            var control = controls[controlId];
            var detail = existing.FirstOrDefault(item => item.ControlDataId == controlId);
            if (detail is null)
            {
                detail = new WfActivityDetail
                {
                    ProcessId = control.ProcessId,
                    AssignmentID = assignment.RecId,
                    ControlId = control.ControlId,
                    ControlDataId = control.RecId,
                    Name = FirstText(control.Name, control.Code),
                    NameAlias = FirstText(control.NameAlias, control.Name, control.Code),
                    SortOrder = control.SortOrder,
                    Score = control.Score,
                    DataAreaId = request.DataAreaId
                };
                _context.WfActivityDetails.Add(detail);
            }

            detail.ControlValue = value;
            detail.Value = value;
            detail.ValueAlias = value;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
