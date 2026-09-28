using System.Globalization;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Execution;
using IAX.IXApi.Modules.Workflow.Operators;
using IAX.IXApi.Modules.Workflow.Performers;
using IAX.IXApi.Modules.Workflow.Processes;
using IAX.IXApi.Modules.Workflow.Steps;
using IAX.IXApi.Modules.Workflow.Transitions;
using IAX.IXApi.Modules.Workflow.Variables;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Workflow.Requests;

public partial class WfRequestService
{
    private sealed record WorkflowStartPlan(long StepId, List<WfProcessVariable> Variables, List<WfAssignment> Assignments);

    // Read-only preparation: the submission coordinator owns all persistence.
    private async Task<WorkflowStartPlan> BuildWorkflowStartPlanAsync(
        WfRequest request, PreparedSubmission prepared, CancellationToken ct)
    {
        var variables = await _context.Set<WfVariable>().AsNoTracking().Include(item => item.DataType)
            .Where(item => item.ProcessId == request.ProcessId && item.IsActive && !item.IsDeleted)
            .ToListAsync(ct);
        var controlIds = prepared.Form.Controls.Select(item => item.RequestControlId).ToList();
        var visibleIds = prepared.Visible.Select(item => item.RequestControlId).ToHashSet();
        var mappings = await _context.Set<WfRequestMappingVariable>().AsNoTracking()
            .Where(item => item.IsActive && !item.IsDeleted &&
                (item.RequestControl.ProcessId == request.ProcessId || item.Variable.ProcessId == request.ProcessId))
            .ToListAsync(ct);
        Dictionary<long, string> values;
        try
        {
            values = WorkflowVariableBindings.Bind(variables.Select(item => item.RecId), prepared.Values,
                visibleIds, mappings.Select(item => (item.RequestControlId, item.VariableId)));
        }
        catch (InvalidOperationException ex) { throw ConfigurationError(ex.Message); }
        foreach (var variable in variables)
        {
            if (variable.DataType == null || !variable.DataType.IsActive || variable.DataType.IsDeleted)
                throw ConfigurationError($"Variable {variable.RecId} uses an inactive data type.");
            try { WorkflowConditionEvaluator.Evaluate(variable.DataType.Code, "EQ", values[variable.RecId],
                string.IsNullOrEmpty(values[variable.RecId]) ? DefaultOperand(variable.DataType.Code) : values[variable.RecId]); }
            catch (Exception ex) when (ex is FormatException or OverflowException or InvalidOperationException)
            { throw ConfigurationError($"Variable {variable.RecId}: {ex.Message}"); }
        }

        var routes = await _context.Set<WfTransition>().AsNoTracking()
            .Where(item => item.ProcessId == request.ProcessId && item.IsActive && !item.IsDeleted && item.ActivityId == null)
            .OrderBy(item => item.SortOrder).ThenBy(item => item.RecId).ToListAsync(ct);
        var operators = await _context.Set<WfOperator>().AsNoTracking()
            .Where(item => item.IsActive && !item.IsDeleted).ToDictionaryAsync(item => item.RecId, ct);
        var matches = new List<WfTransition>();
        foreach (var route in routes)
        {
            if (!values.ContainsKey(route.VariableId) || !operators.TryGetValue(route.OperatorId, out var op) ||
                route.RequestControlId.HasValue && !controlIds.Contains(route.RequestControlId.Value))
                throw ConfigurationError($"Transition {route.RecId} has an unavailable variable, operator or request control.");
            if (route.RequestControlId.HasValue && !visibleIds.Contains(route.RequestControlId.Value)) continue;
            try
            {
                if (WorkflowConditionEvaluator.Evaluate(variables.Single(item => item.RecId == route.VariableId).DataType.Code,
                    op.Code, values[route.VariableId], route.Value)) matches.Add(route);
            }
            catch (Exception ex) when (ex is FormatException or OverflowException or InvalidOperationException or System.Text.Json.JsonException)
            { throw ConfigurationError($"Transition {route.RecId}: {ex.Message}"); }
        }
        var availableSteps = await _context.WfSteps.AsNoTracking()
            .Where(item => item.ProcessId == request.ProcessId && item.IsActive && !item.IsDeleted)
            .OrderBy(item => item.SortOrder == 0 ? 1 : 0)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.RecId)
            .ToListAsync(ct);
        var startingStepId = SelectStartingStepId(matches, availableSteps);
        var step = availableSteps.Single(item => item.RecId == startingStepId);
        var activities = await _context.Set<WfActivity>().AsNoTracking().Include(item => item.Performer).ThenInclude(item => item.PerformerType)
            .Where(item => item.StepId == step.RecId && item.IsActive && !item.IsDeleted).ToListAsync(ct);
        if (activities.Count == 0) throw ConfigurationError("The starting step has no active activities.");
        var assignments = new List<WfAssignment>();
        foreach (var activity in activities)
        {
            var employees = await ResolveStartingPerformersAsync(activity.Performer, request, prepared, ct);
            foreach (var employee in employees)
                assignments.Add(new WfAssignment
                {
                    RequestId = request.RecId, StepId = step.RecId, ActivityId = activity.RecId,
                    UserId = employee, AssignDate = request.RequestDate, IsFinished = false,
                    AutoPassing = activity.IsAutoPassEnabled, AutoPassingHrs = activity.AutoPassAfterHours,
                    Automatically = false, Transferred = false,
                    Score = activity.Score, DataAreaId = request.DataAreaId, IsActive = true
                });
        }
        var runtimeVariables = variables.Select(variable => new WfProcessVariable
        {
            RequestId = request.RecId, VariableId = variable.RecId,
            VariableValue = string.IsNullOrEmpty(values[variable.RecId]) ? null : values[variable.RecId],
            SortOrder = variable.SortOrder,
            DataAreaId = request.DataAreaId
        }).ToList();
        return new WorkflowStartPlan(step.RecId, runtimeVariables, assignments);
    }

    internal static long SelectStartingStepId(
        IEnumerable<WfTransition> matchingRoutes,
        IEnumerable<WfStep> availableSteps)
    {
        var steps = availableSteps.ToList();
        var routedStepId = matchingRoutes.FirstOrDefault()?.StepId;
        if (routedStepId.HasValue)
        {
            if (steps.Any(item => item.RecId == routedStepId.Value)) return routedStepId.Value;
            throw ConfigurationError("The starting transition targets an unavailable step.");
        }

        var defaultStep = steps
            .OrderBy(item => item.SortOrder == 0 ? 1 : 0)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.RecId)
            .FirstOrDefault()
            ?? throw ConfigurationError("The process has no active starting step.");
        return defaultStep.RecId;
    }

    private static string DefaultOperand(string? code) => code?.Trim().ToUpperInvariant() switch
    { "INT" or "DEC" => "0", "BOOL" => "false", "DT" => "2000-01-01", _ => string.Empty };

    private async Task<List<long>> ResolveStartingPerformersAsync(WfPerformer performer, WfRequest request, PreparedSubmission prepared, CancellationToken ct)
    {
        if (performer == null || !performer.IsActive || performer.IsDeleted || performer.PerformerType == null || !performer.PerformerType.IsActive || performer.PerformerType.IsDeleted)
            throw ConfigurationError("An activity has an unavailable performer.");
        var kind = ResolvePerformerKind(performer);
        var employees = kind switch
        {
            "ORGANIZATIONAL" => await ResolveOrganizationalPerformerAsync(performer, request, ct),
            "REQUEST_CONTROL" or "REQUEST" or "REQUESTFIELD" =>
                ResolveRequestControlPerformer(performer, prepared),
            "USER" or "USERS" => await ResolveUserPerformerAsync(performer.RecId, ct),
            "ACTIVITY_CONTROL" => throw ConfigurationError(
                $"Performer {performer.RecId} uses an activity control, which is unavailable when a request starts."),
            "QUERY" or "QUERY_DATABASE" => throw ConfigurationError(
                $"Performer {performer.RecId} database-query execution is not configured."),
            _ => throw ConfigurationError($"Performer {performer.RecId} uses an unsupported resolution type.")
        };

        var ids = employees.ToList();
        var validIds = await _context.Set<HcmWorker>().AsNoTracking()
            .Where(item => ids.Contains(item.RecId) && item.IsActive && !item.IsDeleted)
            .Select(item => item.RecId).ToListAsync(ct);
        if (ids.Count == 0 || validIds.Count != ids.Count)
            throw ConfigurationError($"Performer {performer.RecId} has no eligible employees or contains an unavailable employee.");
        return validIds;
    }

    private static string ResolvePerformerKind(WfPerformer performer) => performer.PerformerTypeId switch
    {
        1 => "ORGANIZATIONAL",
        2 => "REQUEST_CONTROL",
        3 => "ACTIVITY_CONTROL",
        4 => "USER",
        5 => "QUERY_DATABASE",
        _ => performer.PerformerType.Code?.Trim().ToUpperInvariant() ?? string.Empty
    };

    private async Task<HashSet<long>> ResolveOrganizationalPerformerAsync(
        WfPerformer performer,
        WfRequest request,
        CancellationToken ct)
    {
        var employees = new HashSet<long>();
        var managerLevels = new[]
        {
            performer.IsManager1,
            performer.IsManager2,
            performer.IsManager3,
            performer.IsManager4
        };
        if (managerLevels.Count(selected => selected) > 1)
            throw ConfigurationError($"Performer {performer.RecId} selects multiple manager levels.");

        var applicantId = request.EmployeeId;
        var employeeId = request.RequestForHcmWorkerId ?? request.EmployeeId;
        if (performer.IsApplicant && applicantId.HasValue) employees.Add(applicantId.Value);
        if (performer.IsEmployee && employeeId.HasValue) employees.Add(employeeId.Value);

        var managerLevel = Array.FindIndex(managerLevels, selected => selected) + 1;
        if (managerLevel > 0)
        {
            if (!employeeId.HasValue)
                throw ConfigurationError($"Performer {performer.RecId} requires a request employee.");
            employees.Add(await ResolveManagerAsync(employeeId.Value, managerLevel, request.RequestDate, performer.RecId, ct));
        }

        if (employees.Count == 0)
            throw ConfigurationError($"Performer {performer.RecId} has no organizational role selected.");
        return employees;
    }

    private HashSet<long> ResolveRequestControlPerformer(WfPerformer performer, PreparedSubmission prepared)
    {
        var relatedField = performer.RelatedField;
        var control = relatedField.HasValue
            ? prepared.Visible.FirstOrDefault(item => item.RequestControlId == relatedField.Value)
            : null;
        if (control == null ||
            !string.Equals(control.ReferenceType, "Employee", StringComparison.OrdinalIgnoreCase) ||
            !long.TryParse(
                prepared.Values.GetValueOrDefault(relatedField!.Value),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var employeeId))
            throw ConfigurationError($"Performer {performer.RecId} requires a visible employee request field.");
        return [employeeId];
    }

    private async Task<HashSet<long>> ResolveUserPerformerAsync(long performerId, CancellationToken ct) =>
        (await _context.Set<WfPerformerUsers>().AsNoTracking()
            // WfUsersPerformers is a legacy link table. IsActive is deliberately
            // ignored by its EF configuration because that column does not exist.
            .Where(item => item.PerformerId == performerId && !item.IsDeleted)
            .Select(item => item.UserID)
            .ToListAsync(ct))
        .ToHashSet();

    private async Task<long> ResolveManagerAsync(
        long employeeId,
        int managerLevel,
        DateTime effectiveDate,
        long performerId,
        CancellationToken ct)
    {
        var currentId = employeeId;
        var asOf = DateOnly.FromDateTime(effectiveDate);
        for (var level = 1; level <= managerLevel; level++)
        {
            var managerId = await _context.Set<HcmWorkerOrganizationAssignmentV1>()
                .AsNoTracking()
                .Where(assignment =>
                    assignment.HcmWorkerId == currentId &&
                    assignment.IsPrimary &&
                    assignment.IsActive &&
                    !assignment.IsDeleted &&
                    assignment.ValidFrom <= asOf &&
                    (!assignment.ValidTo.HasValue || assignment.ValidTo.Value >= asOf))
                .OrderByDescending(assignment => assignment.ValidFrom)
                .ThenByDescending(assignment => assignment.RecId)
                .Select(assignment => (long?)assignment.HcmManagerWorkerId)
                .FirstOrDefaultAsync(ct);
            if (!managerId.HasValue)
                throw ConfigurationError(
                    $"Performer {performerId} cannot resolve manager level {managerLevel} for employee {employeeId}.");
            currentId = managerId.Value;
        }
        return currentId;
    }

    private static DynamicRequestValidationException ConfigurationError(string message) => new([
        new ValidationResult { RequestControlId = 0, ControlName = "Workflow configuration", ErrorMessage = message, Severity = "Error" }
    ]);
}
