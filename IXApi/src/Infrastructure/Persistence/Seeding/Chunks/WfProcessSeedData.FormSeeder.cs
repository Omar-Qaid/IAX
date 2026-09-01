using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Performers;
using IAX.IXApi.Modules.Workflow.Processes;
using IAX.IXApi.Modules.Workflow.PrintTemplates;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Steps;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

public sealed partial class WfProcessSeedData
{
    private static async Task SeedFormAsync(
        ApplicationDbContext db,
        FormSeedDefinition definition,
        string owner,
        CancellationToken ct)
    {
        var process = await db.WfProcesses.IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Code == definition.Code, ct);
        if (process is null)
        {
            process = new WfProcess
            {
                Code = definition.Code,
                Name = definition.Name,
                Description = definition.Description,
                CategoryId = definition.CategoryId,
                PriorityId = definition.PriorityId,
                ProcessTypeId = definition.ProcessTypeId,
                CanRepeat = true,
                MandatoryDocs = false,
                SortOrder = definition.SortOrder,
                IsActive = true,
                CreatedBy = owner,
                OwnerAccountId = owner,
            };
            db.WfProcesses.Add(process);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            process.Name = definition.Name;
            process.Description = definition.Description;
            process.CategoryId = definition.CategoryId;
            process.PriorityId = definition.PriorityId;
            process.ProcessTypeId = definition.ProcessTypeId;
            process.SortOrder = definition.SortOrder;
            process.IsActive = true;
            process.IsDeleted = false;
            await db.SaveChangesAsync(ct);
        }

        var controlsByCode = await db.WfRequestControls.IgnoreQueryFilters()
            .Where(x => x.ProcessId == process.RecId && x.Code != null)
            .ToDictionaryAsync(x => x.Code!, StringComparer.OrdinalIgnoreCase, ct);

        foreach (var controlDefinition in definition.Controls)
        {
            if (!controlsByCode.TryGetValue(controlDefinition.Code, out var control))
            {
                control = RequestControl(
                    process.RecId,
                    controlDefinition.ControlId,
                    controlDefinition.Code,
                    controlDefinition.Name,
                    controlDefinition.Description,
                    checked((byte)(controlsByCode.Count + 1)),
                    owner,
                    controlDefinition.Required);
                db.WfRequestControls.Add(control);
                await db.SaveChangesAsync(ct);
                controlsByCode.Add(controlDefinition.Code, control);
            }
            else
            {
                control.ControlId = controlDefinition.ControlId;
                control.Name = controlDefinition.Name;
                control.Description = controlDefinition.Description;
                control.ValidationRules = controlDefinition.Required ? RequiredRule : null;
                control.IsActive = true;
                control.IsDeleted = false;
            }

            if (controlDefinition.Required && !await db.WfRequestControlsValidations.IgnoreQueryFilters()
                    .AnyAsync(x => x.RequestControlId == control.RecId && x.ValidationType == "Required", ct))
            {
                db.WfRequestControlsValidations.Add(RequestValidation(control, control.SortOrder, owner));
            }

            if (controlDefinition.Options is { Count: > 0 })
            {
                var existingValues = await db.WfRequestControlsOptions.IgnoreQueryFilters()
                    .Where(x => x.RequestControlId == control.RecId)
                    .Select(x => x.Value)
                    .ToListAsync(ct);
                var missingOptions = controlDefinition.Options
                    .Where(option => !existingValues.Contains(option.Value, StringComparer.OrdinalIgnoreCase))
                    .Select((option, index) => RequestOption(
                        control.RecId,
                        option.Value,
                        option.Name,
                        existingValues.Count + index + 1,
                        owner));
                db.WfRequestControlsOptions.AddRange(missingOptions);
            }
        }
        await db.SaveChangesAsync(ct);

        var performerTypeId = await db.WfPerformerTypes.IgnoreQueryFilters()
            .OrderBy(x => x.RecId)
            .Select(x => (short?)x.RecId)
            .FirstOrDefaultAsync(ct) ?? throw Missing("WfPerformerType");
        var activityTypeId = await db.WfActivityTypes.IgnoreQueryFilters()
            .Where(x => x.RecId == 2)
            .Select(x => (byte?)x.RecId)
            .FirstOrDefaultAsync(ct) ?? throw Missing("WfActivityType 2");

        foreach (var (stepDefinition, index) in definition.Steps.Select((step, index) => (step, index)))
        {
            var performer = await db.WfPerformers.IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.Code == stepDefinition.PerformerCode, ct);
            if (performer is null)
            {
                performer = new WfPerformer
                {
                    Code = stepDefinition.PerformerCode,
                    Name = stepDefinition.PerformerName,
                    PerformerTypeId = performerTypeId,
                    IsActive = true,
                    CreatedBy = owner,
                    OwnerAccountId = owner,
                };
                db.WfPerformers.Add(performer);
                await db.SaveChangesAsync(ct);
            }

            var step = await db.WfSteps.IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.ProcessId == process.RecId && x.Code == stepDefinition.Code, ct);
            if (step is null)
            {
                step = new WfStep
                {
                    ProcessId = process.RecId,
                    Code = stepDefinition.Code,
                    Name = stepDefinition.Name,
                    Description = stepDefinition.Name,
                    SortOrder = checked((byte)(index + 1)),
                    IsActive = true,
                    CreatedBy = owner,
                    OwnerAccountId = owner,
                };
                db.WfSteps.Add(step);
                await db.SaveChangesAsync(ct);
            }

            var activityCode = $"{stepDefinition.Code}_ACTIVITY";
            var activity = await db.WfActivities.IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.StepId == step.RecId && x.Code == activityCode, ct);
            if (activity is null)
            {
                activity = new WfActivity
                {
                    StepId = step.RecId,
                    ActivityTypeId = activityTypeId,
                    PerformerId = performer.RecId,
                    Code = activityCode,
                    Name = stepDefinition.Name,
                    Description = stepDefinition.Name,
                    ShowPreviousDocs = true,
                    ShowPreviousSteps = true,
                    AlertingBySystem = true,
                    IsActive = true,
                    CreatedBy = owner,
                    OwnerAccountId = owner,
                };
                db.WfActivities.Add(activity);
                await db.SaveChangesAsync(ct);
            }

            var approvalCode = $"{activityCode}_DECISION";
            if (!await db.WfActivityControls.IgnoreQueryFilters()
                    .AnyAsync(x => x.ActivityId == activity.RecId && x.Code == approvalCode, ct))
            {
                var approval = ActivityControl(
                    process.RecId, activity.RecId, 6, approvalCode, "الاعتماد", 1, owner, true);
                var notes = ActivityControl(
                    process.RecId, activity.RecId, 3, $"{activityCode}_NOTES", "ملاحظات", 2, owner, false);
                db.WfActivityControls.AddRange(approval, notes);
                await db.SaveChangesAsync(ct);
                db.WfActivityControlsOptions.AddRange(
                    ActivityOption(approval.RecId, "نعم", "نعم", 1, owner),
                    ActivityOption(approval.RecId, "لا", "لا", 2, owner));
                db.WfActivityControlsValidations.Add(ActivityValidation(approval, owner));
                await db.SaveChangesAsync(ct);
            }
        }

        await SeedPrintTemplateAsync(db, process, definition, controlsByCode, owner, ct);
    }

    private static async Task SeedPrintTemplateAsync(
        ApplicationDbContext db,
        WfProcess process,
        FormSeedDefinition definition,
        IReadOnlyDictionary<string, WfRequestControl> controlsByCode,
        string owner,
        CancellationToken ct)
    {
        var template = await db.WfPrintTemplates.IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                x => x.ProcessId == process.RecId && x.Code == definition.PrintTemplateCode,
                ct);
        if (template is null)
        {
            var hasDefault = await db.WfPrintTemplates.IgnoreQueryFilters()
                .AnyAsync(x => x.ProcessId == process.RecId && x.IsDefault && x.IsActive && !x.IsDeleted, ct);
            template = new WfPrintTemplate
            {
                ProcessId = process.RecId,
                Code = definition.PrintTemplateCode,
                Name = definition.Name,
                Description = definition.Description,
                PageSize = "A4",
                Orientation = "portrait",
                Language = "ar",
                IsDefault = !hasDefault,
                Status = WfPrintTemplateStatus.Published,
                IsActive = true,
                CreatedBy = owner,
                OwnerAccountId = owner,
            };
            db.WfPrintTemplates.Add(template);
            await db.SaveChangesAsync(ct);
        }

        var version = await db.WfPrintTemplateVersions.IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.TemplateId == template.RecId && x.VersionNo == 1, ct);
        if (version is null)
        {
            var document = definition.BuildPrintTemplate(controlsByCode);
            var validationErrors = new PrintTemplateDocumentValidator().Validate(document);
            if (validationErrors.Count > 0)
                throw new InvalidOperationException(
                    $"Invalid print template '{definition.PrintTemplateCode}': {string.Join("; ", validationErrors)}");

            version = new WfPrintTemplateVersion
            {
                TemplateId = template.RecId,
                VersionNo = 1,
                TemplateJson = JsonSerializer.Serialize(document, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
                IsPublished = true,
                PublishedBy = owner,
                PublishedAt = DateTime.UtcNow,
                CreatedBy = owner,
                OwnerAccountId = owner,
            };
            db.WfPrintTemplateVersions.Add(version);
            await db.SaveChangesAsync(ct);
        }

        if (template.CurrentVersionId != version.RecId
            || template.Status != WfPrintTemplateStatus.Published
            || !template.IsActive
            || template.IsDeleted)
        {
            template.CurrentVersionId = version.RecId;
            template.Status = WfPrintTemplateStatus.Published;
            template.IsActive = true;
            template.IsDeleted = false;
            await db.SaveChangesAsync(ct);
        }
    }
}

