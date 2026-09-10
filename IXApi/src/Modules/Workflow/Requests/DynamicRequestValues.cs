namespace IAX.IXApi.Modules.Workflow.Requests;

public static class DynamicRequestValues
{
    // Call after rejecting duplicate and foreign control IDs. An explicit empty
    // answer clears an editable default; read-only values always come from configuration.
    public static Dictionary<long, string> Normalize(
        IEnumerable<DynamicRequestControlDto> controls,
        IEnumerable<DynamicRequestValueDto> submitted)
    {
        var answers = submitted.ToDictionary(item => item.RequestControlId, item => item.Value?.Trim() ?? string.Empty);
        return controls.ToDictionary(control => control.RequestControlId, control =>
            control.ReadOnly || !answers.TryGetValue(control.RequestControlId, out var answer)
                ? control.DefaultValue ?? string.Empty
                : answer);
    }
}
