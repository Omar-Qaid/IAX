using System.Text.Json;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Performers;
using IAX.IXApi.Modules.Workflow.PrintTemplates;
using IAX.IXApi.Shared.Domain.Reporting;
using IAX.IXApi.Modules.Workflow.Processes;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Steps;
using IAX.IXApi.Modules.Workflow.Variables;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

public sealed partial class WfProcessSeedData
{
    private const string PaymentPrintTemplateCode = "PAYMENT_REQUEST_AR";

    private sealed record PaymentControlDefinition(
        byte ControlId,
        string Code,
        string Name,
        string NameAlias,
        string Description,
        byte SortOrder,
        bool Required = true);

    private sealed record PaymentOptionDefinition(
        string ControlCode,
        string Value,
        string Name,
        string NameAlias,
        int SortOrder);

    private sealed record PaymentStepDefinition(
        string Code,
        string Name,
        string NameAlias,
        string PerformerCode,
        string PerformerName,
        string PerformerNameAlias);

    private static async Task ReconcilePaymentRequestAndPrintTemplateAsync(
        ApplicationDbContext db,
        WfProcess process,
        string owner,
        CancellationToken ct)
    {
        process.Name = "Payment Request";
        process.NameAlias = "طلب الصرف";
        process.Description = "نموذج طلب صرف ومراجعة واعتماد المبالغ المالية.";
        process.IsActive = true;
        process.IsDeleted = false;

        var locationControlId = await LocationControlIdAsync(db, ct);
        PaymentControlDefinition[] definitions =
        [
            new(11, "PAYMENT_REQUEST_TYPE", "Payment request type", "نوع طلب الصرف", "العهدة النقدية أو الاتفاقيات والعقود والموردين", 1),
            new(4, "PAYMENT_REQUEST_DATE", "Request date", "التاريخ", "تاريخ طلب الصرف", 2),
            new(locationControlId, "PAYMENT_SITE", "Location", "الموقع", "موقع أو إدارة الشركة مقدمة الطلب", 3),
            new(2, "PAYMENT_DEPARTMENT", "Administrative department", "القسم الإداري", "القسم الإداري مقدم الطلب", 4),
            new(9, "PAYMENT_DETAILS", "Payment details", "بيانات الصرف", "تفاصيل الفواتير والتحويل أو الصرف", 5),
            new(1, "PAYMENT_GRAND_TOTAL", "Grand total", "الإجمالي", "إجمالي مبلغ طلب الصرف", 6),
        ];

        var controls = await db.WfRequestControls.IgnoreQueryFilters()
            .Where(x => x.ProcessId == process.RecId && x.Code != null)
            .ToDictionaryAsync(x => x.Code!, StringComparer.OrdinalIgnoreCase, ct);

        foreach (var definition in definitions)
        {
            if (!controls.TryGetValue(definition.Code, out var control))
            {
                control = RequestControl(
                    process.RecId,
                    definition.ControlId,
                    definition.Code,
                    definition.Name,
                    definition.Description,
                    definition.SortOrder,
                    owner,
                    definition.Required);
                db.WfRequestControls.Add(control);
                await db.SaveChangesAsync(ct);
                controls.Add(definition.Code, control);
            }

            control.ControlId = definition.ControlId;
            control.Name = definition.Name;
            control.NameAlias = definition.NameAlias;
            control.Description = definition.Description;
            control.SortOrder = definition.SortOrder;
            control.ValidationRules = definition.Required ? RequiredRule : null;
            control.IsActive = true;
            control.IsDeleted = false;

            var validation = await db.WfRequestControlsValidations.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.RequestControlId == control.RecId && x.ValidationType == "Required", ct);
            if (definition.Required)
            {
                if (validation is null)
                {
                    validation = RequestValidation(control, definition.SortOrder, owner);
                    db.WfRequestControlsValidations.Add(validation);
                }
                validation.ErrorMessage = $"{definition.Name} is required.";
                validation.ErrorMessageAlias = $"حقل {definition.NameAlias} مطلوب.";
                validation.SortOrder = definition.SortOrder;
                validation.IsActive = true;
                validation.IsDeleted = false;
            }
        }

        PaymentOptionDefinition[] optionDefinitions =
        [
            new("PAYMENT_REQUEST_TYPE", "CashAdvance", "Cash advance", "العهدة النقدية", 1),
            new("PAYMENT_REQUEST_TYPE", "ContractsVendors", "Agreements, contracts and vendors", "الاتفاقيات والعقود والموردين", 2),
            new("PAYMENT_DETAILS", "sequence", "No.", "م", 1),
            new("PAYMENT_DETAILS", "beneficiary", "Beneficiary name", "اسم المستفيد", 2),
            new("PAYMENT_DETAILS", "invoice_number", "Invoice number", "رقم الفاتورة", 3),
            new("PAYMENT_DETAILS", "invoice_amount", "Invoice amount", "قيمة الفاتورة", 4),
            new("PAYMENT_DETAILS", "vat", "VAT", "الضريبة", 5),
            new("PAYMENT_DETAILS", "total", "Total", "الإجمالي", 6),
            new("PAYMENT_DETAILS", "payment_statement", "Transfer / payment statement", "بيان التحويل / الصرف", 7),
        ];

        foreach (var definition in optionDefinitions)
        {
            var requestControl = controls[definition.ControlCode];
            var option = await db.WfRequestControlsOptions.IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.RequestControlId == requestControl.RecId && x.Value == definition.Value,
                    ct);
            if (option is null)
            {
                option = RequestOption(
                    requestControl.RecId,
                    definition.Value,
                    definition.Name,
                    definition.SortOrder,
                    owner);
                db.WfRequestControlsOptions.Add(option);
            }
            option.Name = definition.Name;
            option.NameAlias = definition.NameAlias;
            option.SortOrder = definition.SortOrder;
            option.IsActive = true;
            option.IsDeleted = false;
        }

        (string Code, string Name, string NameAlias)[] variableDefinitions =
        [
            ("PAYMENT_REQUEST_TYPE_VAR", "Payment request type", "نوع طلب الصرف"),
            ("PAYMENT_AUDIT_DECISION", "Audit decision", "قرار مسؤول المتابعة والتدقيق"),
            ("PAYMENT_ACCOUNTANT_DECISION", "Accountant decision", "قرار المحاسب"),
            ("PAYMENT_ACCOUNTING_REVIEW_DECISION", "Accounting review decision", "قرار مدير أول المحاسبة والمراجعة"),
            ("PAYMENT_FINANCE_MANAGER_DECISION", "Finance manager decision", "قرار مدير الإدارة المالية"),
            ("PAYMENT_EXECUTIVE_DIRECTOR_DECISION", "Executive director decision", "قرار المدير التنفيذي للموارد البشرية والخدمات المساندة"),
            ("PAYMENT_CEO_DECISION", "Chief executive officer decision", "قرار الرئيس التنفيذي"),
        ];
        var variables = await db.WfVariables.IgnoreQueryFilters()
            .Where(x => x.ProcessId == process.RecId && x.Code != null)
            .ToDictionaryAsync(x => x.Code!, StringComparer.OrdinalIgnoreCase, ct);
        foreach (var definition in variableDefinitions)
        {
            if (!variables.TryGetValue(definition.Code, out var variable))
                throw Missing($"payment variable {definition.Code}");
            variable.Name = definition.Name;
            variable.NameAlias = definition.NameAlias;
            variable.Description = definition.NameAlias;
            variable.IsActive = true;
            variable.IsDeleted = false;
        }

        PaymentStepDefinition[] stepDefinitions =
        [
            new("PAYMENT_AUDIT_STEP", "Follow-up and Audit Officer", "مسؤول المتابعة والتدقيق", "PAYMENT_AUDITOR", "Payment Request Auditor", "مسؤول المتابعة والتدقيق - طلب الصرف"),
            new("PAYMENT_ACCOUNTANT_STEP", "Accountant", "المحاسب", "PAYMENT_ACCOUNTANT", "Payment Request Accountant", "المحاسب - طلب الصرف"),
            new("PAYMENT_ACCOUNTING_REVIEW_STEP", "Senior Accounting and Review Manager (Assigned)", "مدير أول المحاسبة والمراجعة (المكلف)", "PAYMENT_ACCOUNTING_REVIEW_MANAGER", "Payment Request Accounting Review Manager", "مدير أول المحاسبة والمراجعة - طلب الصرف"),
            new("PAYMENT_FINANCE_MANAGER_STEP", "Finance Department Manager", "مدير الإدارة المالية", "PAYMENT_FINANCE_MANAGER", "Payment Request Finance Manager", "مدير الإدارة المالية - طلب الصرف"),
            new("PAYMENT_EXECUTIVE_DIRECTOR_STEP", "Executive Director of Human Resources and Support Services", "المدير التنفيذي للموارد البشرية والخدمات المساندة", "PAYMENT_EXECUTIVE_DIRECTOR", "Payment Request Executive Director", "المدير التنفيذي للموارد البشرية والخدمات المساندة - طلب الصرف"),
            new("PAYMENT_CEO_STEP", "Chief Executive Officer", "الرئيس التنفيذي", "PAYMENT_CEO", "Payment Request Chief Executive Officer", "الرئيس التنفيذي - طلب الصرف"),
            new("PAYMENT_REJECTED_STEP", "Payment Request Rejected", "إنهاء طلب الصرف بالرفض", "", "", ""),
        ];

        var steps = await db.WfSteps.IgnoreQueryFilters()
            .Where(x => x.ProcessId == process.RecId && x.Code != null)
            .ToDictionaryAsync(x => x.Code!, StringComparer.OrdinalIgnoreCase, ct);
        foreach (var definition in stepDefinitions)
        {
            if (!steps.TryGetValue(definition.Code, out var step))
                throw Missing($"payment step {definition.Code}");
            step.Name = definition.Name;
            step.NameAlias = definition.NameAlias;
            step.Description = definition.NameAlias;
            step.IsActive = true;
            step.IsDeleted = false;

            if (string.IsNullOrWhiteSpace(definition.PerformerCode))
                continue;
            var performer = await db.WfPerformers.IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.Code == definition.PerformerCode, ct)
                ?? throw Missing($"payment performer {definition.PerformerCode}");
            // WfPerformer is a workflow lookup rather than a name/alias master.
            performer.Name = definition.PerformerNameAlias;
            performer.IsActive = true;
            performer.IsDeleted = false;

            var activity = await db.WfActivities.IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.StepId == step.RecId && x.Code == $"{definition.Code}_ACTIVITY", ct)
                ?? throw Missing($"payment activity {definition.Code}_ACTIVITY");
            activity.Name = $"Review and approve - {definition.Name}";
            activity.NameAlias = $"مراجعة واعتماد - {definition.NameAlias}";
            activity.Description = $"مراجعة طلب الصرف بواسطة {definition.NameAlias}";
            activity.IsActive = true;
            activity.IsDeleted = false;

            var activityControls = await db.WfActivityControls.IgnoreQueryFilters()
                .Where(x => x.ActivityId == activity.RecId && x.Code != null)
                .ToListAsync(ct);
            var approval = activityControls.FirstOrDefault(x => x.Code == $"{activity.Code}_DECISION")
                ?? throw Missing($"payment activity control {activity.Code}_DECISION");
            approval.Name = "Approval";
            approval.NameAlias = "الاعتماد";
            approval.Description = "قرار الاعتماد";
            approval.IsActive = true;
            approval.IsDeleted = false;
            var notes = activityControls.FirstOrDefault(x => x.Code == $"{activity.Code}_NOTES")
                ?? throw Missing($"payment activity control {activity.Code}_NOTES");
            notes.Name = "Notes";
            notes.NameAlias = "ملاحظات";
            notes.Description = "ملاحظات الاعتماد";
            notes.IsActive = true;
            notes.IsDeleted = false;

            var approvalOptions = await db.WfActivityControlsOptions.IgnoreQueryFilters()
                .Where(x => x.ActivityControlId == approval.RecId)
                .ToListAsync(ct);
            foreach (var option in approvalOptions)
            {
                if (option.Value == "نعم")
                {
                    option.Name = "Yes";
                    option.NameAlias = "نعم";
                }
                else if (option.Value == "لا")
                {
                    option.Name = "No";
                    option.NameAlias = "لا";
                }
            }

            var activityValidation = await db.WfActivityControlsValidations.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.ActivityControlId == approval.RecId && x.ValidationType == "Required", ct);
            if (activityValidation is not null)
            {
                activityValidation.Name = "Approval required";
                activityValidation.NameAlias = "الاعتماد مطلوب";
                activityValidation.IsActive = true;
                activityValidation.IsDeleted = false;
            }
        }

        await db.SaveChangesAsync(ct);
        await SeedPaymentPrintTemplateAsync(db, process, controls, steps, owner, ct);
    }

    private static async Task SeedPaymentPrintTemplateAsync(
        ApplicationDbContext db,
        WfProcess process,
        IReadOnlyDictionary<string, WfRequestControl> controls,
        IReadOnlyDictionary<string, WfStep> steps,
        string owner,
        CancellationToken ct)
    {
        var template = await db.ReportTemplates.IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                x => x.RefTableId == PrintTemplateService.WorkflowProcessTableId && x.RefRecId == process.RecId && x.Code == PaymentPrintTemplateCode,
                ct);
        if (template is null)
        {
            var hasDefault = await db.ReportTemplates.IgnoreQueryFilters()
                .AnyAsync(x => x.RefTableId == PrintTemplateService.WorkflowProcessTableId && x.RefRecId == process.RecId && x.IsDefault && x.IsActive && !x.IsDeleted, ct);
            template = new ReportTemplate
            {
                RefTableId = PrintTemplateService.WorkflowProcessTableId,
                RefRecId = process.RecId,
                Code = PaymentPrintTemplateCode,
                Name = "Payment Request Form",
                NameAlias = "نموذج طلب الصرف",
                Description = "نموذج طلب الصرف مطابق للتصميم المرجعي المعتمد.",
                PageSize = "A4",
                Orientation = "portrait",
                Language = "ar",
                IsDefault = !hasDefault,
                Status = ReportTemplateStatus.Published,
                IsActive = true,
                CreatedBy = owner,
                OwnerAccountId = owner,
            };
            db.ReportTemplates.Add(template);
            await db.SaveChangesAsync(ct);
        }

        template.Name = "Payment Request Form";
        template.NameAlias = "نموذج طلب الصرف";
        template.Description = "نموذج طلب الصرف مطابق للتصميم المرجعي المعتمد.";
        template.PageSize = "A4";
        template.Orientation = "portrait";
        template.Language = "ar";
        template.Status = ReportTemplateStatus.Published;
        template.IsActive = true;
        template.IsDeleted = false;

        var document = BuildPaymentRequestPrintTemplate(controls, steps);
        var validationErrors = new PrintTemplateDocumentValidator().Validate(document);
        if (validationErrors.Count > 0)
            throw new InvalidOperationException(
                $"Invalid print template '{PaymentPrintTemplateCode}': {string.Join("; ", validationErrors)}");

        var json = JsonSerializer.Serialize(document, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var versions = await db.ReportTemplateVersions.IgnoreQueryFilters()
            .Where(x => x.TemplateId == template.RecId)
            .OrderByDescending(x => x.VersionNo)
            .ToListAsync(ct);
        var version = versions.FirstOrDefault(x => x.TemplateJson == json);
        if (version is null)
        {
            version = new ReportTemplateVersion
            {
                TemplateId = template.RecId,
                VersionNo = (versions.FirstOrDefault()?.VersionNo ?? 0) + 1,
                TemplateJson = json,
                IsPublished = true,
                PublishedBy = owner,
                PublishedAt = DateTime.UtcNow,
                CreatedBy = owner,
                OwnerAccountId = owner,
            };
            db.ReportTemplateVersions.Add(version);
            await db.SaveChangesAsync(ct);
        }

        template.CurrentVersionId = version.RecId;
        await db.SaveChangesAsync(ct);
    }
}
