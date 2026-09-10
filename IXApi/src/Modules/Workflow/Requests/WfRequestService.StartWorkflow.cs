using System.Globalization;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Organization.HcmWorkerManagers;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Execution;
using IAX.IXApi.Modules.Workflow.Operators;
using IAX.IXApi.Modules.Workflow.Performers;
using IAX.IXApi.Modules.Workflow.Processes;
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
        var routeToStart = matches.FirstOrDefault()
            ?? throw ConfigurationError("No starting transition matches the submitted values.");
        var step = await _context.WfSteps.AsNoTracking().SingleOrDefaultAsync(item =>
            item.RecId == routeToStart.StepId && item.ProcessId == request.ProcessId && item.IsActive && !item.IsDeleted, ct)
            ?? throw ConfigurationError("The starting transition targets an unavailable step.");
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
                    Score = activity.Score, DataAreaId = request.DataAreaId, IsActive = true
                });
        }
        var runtimeVariables = variables.Select(variable => new WfProcessVariable
        {
            RequestId = request.RecId, VariableId = variable.RecId, VariableValue = values[variable.RecId],
            DataAreaId = request.DataAreaId
        }).ToList();
        return new WorkflowStartPlan(step.RecId, runtimeVariables, assignments);
    }

    private static string DefaultOperand(string? code) => code?.Trim().ToUpperInvariant() switch
    { "INT" or "DEC" => "0", "BOOL" => "false", "DT" => "2000-01-01", _ => string.Empty };

    private async Task<List<long>> ResolveStartingPerformersAsync(WfPerformer performer, WfRequest request, PreparedSubmission prepared, CancellationToken ct)
    {
        if (performer == null || !performer.IsActive || performer.IsDeleted || performer.PerformerType == null || !performer.PerformerType.IsActive || performer.PerformerType.IsDeleted)
            throw ConfigurationError("An activity has an unavailable performer.");
        var kind = performer.PerformerType.Code?.Trim().ToUpperInvariant();
        if (kind is not ("RELATIONAL" or "REQUEST" or "REQUESTFIELD" or "USERS") ||
            !string.IsNullOrWhiteSpace(performer.SqlTable) || !string.IsNullOrWhiteSpace(performer.SqlWhere) || !string.IsNullOrWhiteSpace(performer.SqlField))
            throw ConfigurationError($"Performer {performer.RecId} uses an unsupported resolution type.");
        var employees = new HashSet<long>();
        var managerLevels = new[] { performer.IsManager1, performer.IsManager2, performer.IsManager3, performer.IsManager4 };
        if (managerLevels.Count(item => item) > 1)
            throw ConfigurationError($"Performer {performer.RecId} selects multiple manager levels.");
        var needsSubject = performer.RelatedField.HasValue || performer.IsApplicant || performer.IsEmployee || managerLevels.Any(item => item) || kind is "REQUEST" or "REQUESTFIELD";
        if (needsSubject)
        {
            var subject = request.EmployeeId;
            if (performer.RelatedField.HasValue)
            {
                if (!prepared.Visible.Any(item => item.RequestControlId == performer.RelatedField.Value) ||
                    !long.TryParse(prepared.Values.GetValueOrDefault(performer.RelatedField.Value), NumberStyles.None, CultureInfo.InvariantCulture, out var fieldEmployee))
                    throw ConfigurationError($"Performer {performer.RecId} requires a visible employee field.");
                subject = fieldEmployee;
            }
            if (!subject.HasValue) throw ConfigurationError($"Performer {performer.RecId} requires a request employee.");
            if (!await _context.Set<HcmWorker>().AsNoTracking().AnyAsync(item => item.RecId == subject.Value && item.IsActive && !item.IsDeleted, ct))
                throw ConfigurationError($"Performer {performer.RecId} references an unavailable employee.");
            if (managerLevels.Any(item => item))
            {
                var level = Array.FindIndex(managerLevels, item => item) + 1;
                var managers = await _context.Set<HcmWorkerManager>().AsNoTracking()
                    .Where(item => item.EmployeeId == subject.Value && item.ManagementLevel.Level == level && item.ManagementLevel.IsActive && !item.ManagementLevel.IsDeleted)
                    .Select(item => item.ManagerId).Distinct().ToListAsync(ct);
                if (managers.Count != 1) throw ConfigurationError($"Performer {performer.RecId} requires exactly one manager at level {level}.");
                employees.Add(managers[0]);
            }
            else employees.Add(subject.Value);
        }
        var explicitEmployees = await _context.Set<WfPerformerUsers>().AsNoTracking()
            .Where(item => item.PerformerId == performer.RecId && !item.IsDeleted)
            .Select(item => item.UserID).ToListAsync(ct);
        employees.UnionWith(explicitEmployees);
        var ids = employees.ToList();
        var validIds = await _context.Set<HcmWorker>().AsNoTracking()
            .Where(item => ids.Contains(item.RecId) && item.IsActive && !item.IsDeleted)
            .Select(item => item.RecId).ToListAsync(ct);
        if (ids.Count == 0 || validIds.Count != ids.Count)
            throw ConfigurationError($"Performer {performer.RecId} has no eligible employees or contains an unavailable employee.");
        return validIds;
    }

    private static DynamicRequestValidationException ConfigurationError(string message) => new([
        new ValidationResult { RequestControlId = 0, ControlName = "Workflow configuration", ErrorMessage = message, Severity = "Error" }
    ]);
}
