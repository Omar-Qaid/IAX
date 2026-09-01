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
                NameAlias = definition.NameAlias,
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
            process.NameAlias = definition.NameAlias;
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
                    controlDefinition.NameAlias,
                    controlDefinition.Description,
                    checked((byte)(controlsByCode.Count + 1)),
                    owner,
                    controlDefinition.Required);
                db.WfRequestControls.Add(control);
                await db.SaveChangesAsync(ct);
                controlsByCode.Add(controlDefinition.Code, control);
            }
            control.ControlId = controlDefinition.ControlId;
            control.Name = controlDefinition.Name;
            control.NameAlias = controlDefinition.NameAlias;
            control.Description = controlDefinition.Description;
            control.ValidationRules = controlDefinition.Required ? RequiredRule : null;
            control.IsActive = true;
            control.IsDeleted = false;

            if (controlDefinition.Required)
            {
                var validation = await db.WfRequestControlsValidations.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(
                        x => x.RequestControlId == control.RecId && x.ValidationType == "Required",
                        ct);
                if (validation is null)
                {
                    validation = RequestValidation(control, control.SortOrder, owner);
                    db.WfRequestControlsValidations.Add(validation);
                }
                validation.ErrorMessage = $"{controlDefinition.Name} is required.";
                validation.ErrorMessageAlias = $"حقل {controlDefinition.NameAlias} مطلوب.";
                validation.IsActive = true;
                validation.IsDeleted = false;
            }

            if (controlDefinition.Options is { Count: > 0 })
            {
                var existingOptions = await db.WfRequestControlsOptions.IgnoreQueryFilters()
                    .Where(x => x.RequestControlId == control.RecId)
                    .ToListAsync(ct);
                foreach (var (optionDefinition, optionIndex) in controlDefinition.Options.Select((option, index) => (option, index)))
                {
                    var option = existingOptions.FirstOrDefault(item =>
                        string.Equals(item.Value, optionDefinition.Value, StringComparison.OrdinalIgnoreCase));
                    if (option is null)
                    {
                        option = RequestOption(
                            control.RecId,
                            optionDefinition.Value,
                            optionDefinition.Name,
                            optionDefinition.NameAlias,
                            optionIndex + 1,
                            owner);
                        db.WfRequestControlsOptions.Add(option);
                    }
                    option.Name = optionDefinition.Name;
                    option.NameAlias = optionDefinition.NameAlias;
                    option.SortOrder = optionIndex + 1;
                    option.IsActive = true;
                    option.IsDeleted = false;
                }
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
            performer.Name = stepDefinition.PerformerName;
            performer.PerformerTypeId = performerTypeId;
            performer.IsActive = true;
            performer.IsDeleted = false;

            var step = await db.WfSteps.IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.ProcessId == process.RecId && x.Code == stepDefinition.Code, ct);
            if (step is null)
            {
                step = new WfStep
                {
                    ProcessId = process.RecId,
                    Code = stepDefinition.Code,
                    Name = stepDefinition.Name,
                    NameAlias = stepDefinition.NameAlias,
                    Description = stepDefinition.NameAlias,
                    SortOrder = checked((byte)(index + 1)),
                    IsActive = true,
                    CreatedBy = owner,
                    OwnerAccountId = owner,
                };
                db.WfSteps.Add(step);
                await db.SaveChangesAsync(ct);
            }
            step.Name = stepDefinition.Name;
            step.NameAlias = stepDefinition.NameAlias;
            step.Description = stepDefinition.NameAlias;
            step.SortOrder = checked((byte)(index + 1));
            step.IsActive = true;
            step.IsDeleted = false;

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
                    Name = $"Review and approve - {stepDefinition.Name}",
                    NameAlias = $"مراجعة واعتماد - {stepDefinition.NameAlias}",
                    Description = stepDefinition.NameAlias,
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
            activity.ActivityTypeId = activityTypeId;
            activity.PerformerId = performer.RecId;
            activity.Name = $"Review and approve - {stepDefinition.Name}";
            activity.NameAlias = $"مراجعة واعتماد - {stepDefinition.NameAlias}";
            activity.Description = stepDefinition.NameAlias;
            activity.IsActive = true;
            activity.IsDeleted = false;

            var approvalCode = $"{activityCode}_DECISION";
            var activityControls = await db.WfActivityControls.IgnoreQueryFilters()
                .Where(x => x.ActivityId == activity.RecId && x.Code != null)
                .ToListAsync(ct);
            var approval = activityControls.FirstOrDefault(x => x.Code == approvalCode);
            if (approval is null)
            {
                approval = ActivityControl(
                    process.RecId, activity.RecId, 6, approvalCode, "Approval", "الاعتماد", 1, owner, true);
                db.WfActivityControls.Add(approval);
                await db.SaveChangesAsync(ct);
            }
            approval.Name = "Approval";
            approval.NameAlias = "الاعتماد";
            approval.Description = "Approval decision";
            approval.IsActive = true;
            approval.IsDeleted = false;

            var notesCode = $"{activityCode}_NOTES";
            var notes = activityControls.FirstOrDefault(x => x.Code == notesCode);
            if (notes is null)
            {
                notes = ActivityControl(
                    process.RecId, activity.RecId, 3, notesCode, "Notes", "ملاحظات", 2, owner, false);
                db.WfActivityControls.Add(notes);
            }
            notes.Name = "Notes";
            notes.NameAlias = "ملاحظات";
            notes.Description = "Approval notes";
            notes.IsActive = true;
            notes.IsDeleted = false;

            var approvalOptions = await db.WfActivityControlsOptions.IgnoreQueryFilters()
                .Where(x => x.ActivityControlId == approval.RecId)
                .ToListAsync(ct);
            foreach (var optionDefinition in new[] { (Value: "نعم", Name: "Yes", NameAlias: "نعم", Order: 1), (Value: "لا", Name: "No", NameAlias: "لا", Order: 2) })
            {
                var option = approvalOptions.FirstOrDefault(x => x.Value == optionDefinition.Value);
                if (option is null)
                {
                    option = ActivityOption(approval.RecId, optionDefinition.Value, optionDefinition.Name,
                        optionDefinition.NameAlias, optionDefinition.Order, owner);
                    db.WfActivityControlsOptions.Add(option);
                }
                option.Name = optionDefinition.Name;
                option.NameAlias = optionDefinition.NameAlias;
                option.SortOrder = optionDefinition.Order;
                option.IsActive = true;
                option.IsDeleted = false;
            }

            var activityValidation = await db.WfActivityControlsValidations.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.ActivityControlId == approval.RecId && x.ValidationType == "Required", ct);
            if (activityValidation is null)
            {
                activityValidation = ActivityValidation(approval, owner);
                db.WfActivityControlsValidations.Add(activityValidation);
            }
            activityValidation.Name = "Approval required";
            activityValidation.NameAlias = "الاعتماد مطلوب";
            activityValidation.IsActive = true;
            activityValidation.IsDeleted = false;
            await db.SaveChangesAsync(ct);
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
                NameAlias = definition.NameAlias,
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

        template.Name = definition.Name;
        template.NameAlias = definition.NameAlias;
        template.Description = definition.Description;

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
        }
        await db.SaveChangesAsync(ct);
    }
}
