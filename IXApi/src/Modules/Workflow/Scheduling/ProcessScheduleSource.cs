using System.Globalization;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Organization.Showrooms;
using IAX.IXApi.Modules.Workflow.Persistence;
using IAX.IXApi.Modules.Workflow.Requests;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Workflow.Scheduling;

public static class ProcessScheduleSource
{
    public static async Task<SubmitDynamicRequestDto> ResolveAsync(IWorkflowDataContext db, long processId,
        string company, ProcessScheduleDto configuration, CancellationToken ct)
    {
        var table = configuration.SourceType switch
        {
            "employee" => "HcmWorker",
            "showroom" => "Showroom",
            "table" => configuration.SourceTable,
            _ => throw new ArgumentException("Choose an employee, showroom or supported table source.")
        };
        if (!long.TryParse(configuration.SourceRecord, out var id) || id <= 0)
            throw new ArgumentException("Choose a valid source record ID.");
        Dictionary<string, string> fields;
        if (table == "HcmWorker")
        {
            var worker = await db.Set<HcmWorker>().AsNoTracking().SingleOrDefaultAsync(
                x => x.RecId == id && x.DataAreaId == company && x.IsActive && !x.IsDeleted, ct)
                ?? throw new ArgumentException("The employee source is unavailable in this company.");
            fields = new(StringComparer.OrdinalIgnoreCase)
            {
                ["RecId"] = Text(worker.RecId), ["PersonnelNumber"] = worker.PersonnelNumber,
                ["DepartmentId"] = Text(worker.DepartmentId), ["ShowroomId"] = Text(worker.ShowroomId)
            };
        }
        else if (table == "Showroom")
        {
            var showroom = await db.Set<Showroom>().AsNoTracking().SingleOrDefaultAsync(
                x => x.RecId == id && x.DataAreaId == company && x.IsActive && !x.IsDeleted, ct)
                ?? throw new ArgumentException("The showroom source is unavailable in this company.");
            fields = new(StringComparer.OrdinalIgnoreCase)
            {
                ["RecId"] = Text(showroom.RecId), ["Name"] = showroom.Name ?? "",
                ["DepartmentId"] = Text(showroom.DepartmentId), ["Location"] = showroom.Location ?? ""
            };
        }
        else throw new ArgumentException("Supported tables are HcmWorker and Showroom. Other sources need a registered resolver.");

        var controls = await db.WfRequestControls.AsNoTracking()
            .Where(x => x.ProcessId == processId && x.IsActive && !x.IsDeleted)
            .Select(x => x.RecId).ToListAsync(ct);
        var result = new SubmitDynamicRequestDto { ProcessId = processId };
        var seen = new HashSet<long>();
        foreach (var mapping in configuration.Mappings)
        {
            if (!long.TryParse(mapping.TargetControlId, out var controlId) || !controls.Contains(controlId) || !seen.Add(controlId))
                throw new ArgumentException("Each mapping must target a different saved request control in this process.");
            if (!fields.TryGetValue(mapping.SourceField, out var value))
                throw new ArgumentException($"Unsupported source field: {mapping.SourceField}.");
            result.Values.Add(new() { RequestControlId = controlId, Value = value });
        }
        return result;
    }

    private static string Text(object? value) => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
}
