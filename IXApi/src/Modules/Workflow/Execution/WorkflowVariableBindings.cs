namespace IAX.IXApi.Modules.Workflow.Execution;

public static class WorkflowVariableBindings
{
    public static Dictionary<long, string> Bind(
        IEnumerable<long> variableIds,
        IReadOnlyDictionary<long, string> normalizedControls,
        IReadOnlySet<long> visibleControlIds,
        IEnumerable<(long ControlId, long VariableId)> mappings)
    {
        var values = variableIds.ToDictionary(id => id, _ => string.Empty);
        var assigned = new HashSet<long>();
        foreach (var mapping in mappings)
        {
            if (!normalizedControls.ContainsKey(mapping.ControlId) || !values.ContainsKey(mapping.VariableId))
                throw new InvalidOperationException("A variable mapping refers to an unavailable control or variable in another process.");
            if (!assigned.Add(mapping.VariableId))
                throw new InvalidOperationException("Each variable must have at most one request-control mapping.");
            if (visibleControlIds.Contains(mapping.ControlId))
                values[mapping.VariableId] = normalizedControls[mapping.ControlId];
        }
        return values;
    }
}
