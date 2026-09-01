using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Requests;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

public sealed partial class WfProcessSeedData
{
    private static WfRequestControl RequestControl(long processId, byte controlId, string code,
        string name, string description, byte order, string owner, bool required = true) => new()
    {
        ProcessId = processId, ControlId = controlId, Code = code, Name = name,
        Description = description, SortOrder = order, ValidationRules = required ? RequiredRule : null,
        IsActive = true, CreatedBy = owner, OwnerAccountId = owner,
    };

    private static WfActivityControl ActivityControl(long processId, long activityId, byte controlId,
        string code, string name, byte order, string owner, bool required) => new()
    {
        ProcessId = processId, ActivityId = activityId, ControlId = controlId, Code = code,
        Name = name, Description = name, SortOrder = order,
        ValidationRules = required ? RequiredRule : null,
        IsActive = true, CreatedBy = owner, OwnerAccountId = owner,
    };

    private static WfRequestControlsOption RequestOption(long id, string value, string name,
        int order, string owner) => new()
    {
        RequestControlId = id, Value = value, Name = name, SortOrder = order,
        IsActive = true, CreatedBy = owner, OwnerAccountId = owner,
    };

    private static WfActivityControlsOption ActivityOption(long id, string value, string name,
        int order, string owner) => new()
    {
        ActivityControlId = id, Value = value, Name = name, SortOrder = order,
        IsActive = true, CreatedBy = owner, OwnerAccountId = owner,
    };

    private static WfRequestControlsValidation RequestValidation(WfRequestControl control,
        int order, string owner) => new()
    {
        RequestControlId = control.RecId, ValidationType = "Required", Value = "true",
        ErrorMessage = $"حقل {control.Name} مطلوب.", Severity = "Error", SortOrder = order,
        IsActive = true, CreatedBy = owner, OwnerAccountId = owner,
    };

    private static WfActivityControlsValidation ActivityValidation(WfActivityControl control,
        string owner) => new()
    {
        ActivityControlId = control.RecId, Code = $"REQ_{control.Code}", Name = "الاعتماد مطلوب",
        ValidationType = "Required", Value = "true", ErrorMessage = "يجب اختيار قرار الاعتماد.",
        Severity = "Error", SortOrder = 1, IsActive = true,
        CreatedBy = owner, OwnerAccountId = owner,
    };

    private static async Task<byte> LocationControlIdAsync(ApplicationDbContext db, CancellationToken ct) =>
        await db.WfControls.IgnoreQueryFilters().Where(x => x.RecId == 21)
            .Select(x => (byte?)x.RecId).FirstOrDefaultAsync(ct) ?? 2;

    private static InvalidOperationException Missing(string value) =>
        new($"Payment Request seed requires {value}.");
}

