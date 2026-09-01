using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Performers;
using IAX.IXApi.Modules.Workflow.Processes;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Steps;
using IAX.IXApi.Modules.Workflow.Transitions;
using IAX.IXApi.Modules.Workflow.Variables;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

public sealed partial class WfProcessSeedData
{
    private static async Task SeedPaymentRequestExampleAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingProcess = await db.WfProcesses.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == ProcessCode, ct);
        if (existingProcess is not null)
        {
            // Request Submission displays only active, non-deleted processes.
            // Reconcile databases seeded by the earlier inactive version.
            if (!existingProcess.IsActive || existingProcess.IsDeleted)
            {
                existingProcess.IsActive = true;
                existingProcess.IsDeleted = false;
                await db.SaveChangesAsync(ct);
            }

            return;
        }

        var category = await db.WfCategories.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.RecId == 6, ct)
            ?? throw Missing("Finance WfCategory 6");
        var priority = await db.WfPriorities.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.RecId == 3, ct)
            ?? throw Missing("WfPriority 3");
        var processType = await db.WfProcessTypes.IgnoreQueryFilters().OrderBy(x => x.RecId).FirstOrDefaultAsync(ct)
            ?? throw Missing("WfProcessType");
        var stringType = await db.WfDataTypes.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.RecId == 2, ct)
            ?? throw Missing("String WfDataType 2");
        var activityType = await db.WfActivityTypes.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.RecId == 2, ct)
            ?? await db.WfActivityTypes.IgnoreQueryFilters().OrderBy(x => x.RecId).FirstOrDefaultAsync(ct)
            ?? throw Missing("WfActivityType");
        var equalsOperator = await db.WfOperators.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.RecId == 5, ct)
            ?? throw Missing("equality WfOperator 5");
        var performerType = await db.WfPerformerTypes.IgnoreQueryFilters().OrderBy(x => x.RecId).FirstOrDefaultAsync(ct)
            ?? throw Missing("WfPerformerType");

        var requiredControls = new byte[] { 1, 2, 3, 4, 6, 9, 11 };
        var foundControls = await db.WfControls.IgnoreQueryFilters()
            .Where(x => requiredControls.Contains(x.RecId)).Select(x => x.RecId).ToListAsync(ct);
        var missingControls = requiredControls.Except(foundControls).ToArray();
        if (missingControls.Length > 0)
            throw Missing($"WfControls {string.Join(", ", missingControls)}");

        var executionStrategy = db.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            // A retry must start with a clean tracker because the previous attempt may
            // have generated identity values before its transaction was rolled back.
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            try
            {
            var process = new WfProcess
            {
                Code = ProcessCode,
                Name = "طلب الصرف",
                Description = "معاملة طلب صرف ومراجعة واعتماد المبالغ المالية.",
                CategoryId = category.RecId,
                PriorityId = priority.RecId,
                ProcessTypeId = processType.RecId,
                CanRepeat = true,
                MandatoryDocs = false,
                SortOrder = 1,
                IsActive = true,
                CreatedBy = owner,
                OwnerAccountId = owner,
            };
            db.WfProcesses.Add(process);
            await db.SaveChangesAsync(ct);

            var controls = new[]
            {
                RequestControl(process.RecId, 11, "PAYMENT_REQUEST_TYPE", "نوع طلب الصرف", "العهدة النقدية أو الاتفاقيات والعقود والموردين", 1, owner),
                RequestControl(process.RecId, 4, "PAYMENT_REQUEST_DATE", "التاريخ", "تاريخ طلب الصرف", 2, owner),
                RequestControl(process.RecId, await LocationControlIdAsync(db, ct), "PAYMENT_SITE", "الموقع", "موقع طلب الصرف", 3, owner),
                RequestControl(process.RecId, 2, "PAYMENT_DEPARTMENT", "القسم الإداري", "القسم الإداري مقدم الطلب", 4, owner),
                RequestControl(process.RecId, 9, "PAYMENT_DETAILS", "بيانات الصرف", "تفاصيل الفواتير والتحويل أو الصرف", 5, owner),
            };
            db.WfRequestControls.AddRange(controls);
            await db.SaveChangesAsync(ct);

            db.WfRequestControlsOptions.AddRange(
                RequestOption(controls[0].RecId, "CashAdvance", "العهدة النقدية", 1, owner),
                RequestOption(controls[0].RecId, "ContractsVendors", "الاتفاقيات والعقود والموردين", 2, owner),
                RequestOption(controls[4].RecId, "sequence", "م", 1, owner),
                RequestOption(controls[4].RecId, "beneficiary", "اسم المستفيد", 2, owner),
                RequestOption(controls[4].RecId, "invoice_number", "رقم الفاتورة", 3, owner),
                RequestOption(controls[4].RecId, "invoice_amount", "قيمة الفاتورة", 4, owner),
                RequestOption(controls[4].RecId, "vat", "الضريبة", 5, owner),
                RequestOption(controls[4].RecId, "total", "الإجمالي", 6, owner),
                RequestOption(controls[4].RecId, "payment_statement", "بيان التحويل / الصرف", 7, owner));
            db.WfRequestControlsValidations.AddRange(controls.Select((control, index) =>
                RequestValidation(control, index + 1, owner)));

            var variableNames = new[]
            {
                ("PAYMENT_REQUEST_TYPE_VAR", "نوع طلب الصرف"),
                ("PAYMENT_AUDIT_DECISION", "قرار مسؤول المتابعة والتدقيق"),
                ("PAYMENT_ACCOUNTANT_DECISION", "قرار المحاسب"),
                ("PAYMENT_ACCOUNTING_REVIEW_DECISION", "قرار مدير أول المحاسبة والمراجعة"),
                ("PAYMENT_FINANCE_MANAGER_DECISION", "قرار مدير الإدارة المالية"),
                ("PAYMENT_EXECUTIVE_DIRECTOR_DECISION", "قرار المدير التنفيذي للموارد البشرية والخدمات المساندة"),
                ("PAYMENT_CEO_DECISION", "قرار الرئيس التنفيذي"),
            };
            var variables = variableNames.Select((item, index) => new WfVariable
            {
                ProcessId = process.RecId,
                DataTypeId = stringType.RecId,
                Code = item.Item1,
                Name = item.Item2,
                Description = item.Item2,
                SortOrder = (byte)(index + 1),
                IsActive = true,
                CreatedBy = owner,
                OwnerAccountId = owner,
            }).ToArray();
            db.WfVariables.AddRange(variables);
            await db.SaveChangesAsync(ct);
            db.WfRequestMappingVariables.Add(new WfRequestMappingVariable
            {
                RequestControlId = controls[0].RecId,
                VariableId = variables[0].RecId,
                SortOrder = 1,
                IsActive = true,
                CreatedBy = owner,
                OwnerAccountId = owner,
            });

            var performerNames = new[]
            {
                ("PAYMENT_AUDITOR", "مسؤول المتابعة والتدقيق - طلب الصرف"),
                ("PAYMENT_ACCOUNTANT", "المحاسب - طلب الصرف"),
                ("PAYMENT_ACCOUNTING_REVIEW_MANAGER", "مدير أول المحاسبة والمراجعة - طلب الصرف"),
                ("PAYMENT_FINANCE_MANAGER", "مدير الإدارة المالية - طلب الصرف"),
                ("PAYMENT_EXECUTIVE_DIRECTOR", "المدير التنفيذي للموارد البشرية والخدمات المساندة - طلب الصرف"),
                ("PAYMENT_CEO", "الرئيس التنفيذي - طلب الصرف"),
            };
            var performers = performerNames.Select(item => new WfPerformer
            {
                Code = item.Item1,
                Name = item.Item2,
                PerformerTypeId = performerType.RecId,
                IsActive = true,
                CreatedBy = owner,
                OwnerAccountId = owner,
            }).ToArray();
            db.WfPerformers.AddRange(performers);
            await db.SaveChangesAsync(ct);

            var stepNames = new[]
            {
                ("PAYMENT_AUDIT_STEP", "مسؤول المتابعة والتدقيق"),
                ("PAYMENT_ACCOUNTANT_STEP", "المحاسب"),
                ("PAYMENT_ACCOUNTING_REVIEW_STEP", "مدير أول المحاسبة والمراجعة (المكلف)"),
                ("PAYMENT_FINANCE_MANAGER_STEP", "مدير الإدارة المالية"),
                ("PAYMENT_EXECUTIVE_DIRECTOR_STEP", "المدير التنفيذي للموارد البشرية والخدمات المساندة"),
                ("PAYMENT_CEO_STEP", "الرئيس التنفيذي"),
                ("PAYMENT_REJECTED_STEP", "إنهاء طلب الصرف بالرفض"),
            };
            var steps = stepNames.Select((item, index) => new WfStep
            {
                ProcessId = process.RecId,
                Code = item.Item1,
                Name = item.Item2,
                Description = item.Item2,
                SortOrder = (byte)(index + 1),
                IsActive = true,
                CreatedBy = owner,
                OwnerAccountId = owner,
            }).ToArray();
            db.WfSteps.AddRange(steps);
            await db.SaveChangesAsync(ct);

            var activities = steps.Take(6).Select((step, index) => new WfActivity
            {
                StepId = step.RecId,
                ActivityTypeId = activityType.RecId,
                PerformerId = performers[index].RecId,
                Code = $"{step.Code}_ACTIVITY",
                Name = index == 5 ? "الاعتماد النهائي للرئيس التنفيذي" : $"مراجعة واعتماد {step.Name}",
                Description = $"مراجعة طلب الصرف بواسطة {step.Name}",
                ShowPreviousDocs = true,
                ShowPreviousSteps = true,
                AlertingBySystem = true,
                IsActive = true,
                CreatedBy = owner,
                OwnerAccountId = owner,
            }).ToArray();
            db.WfActivities.AddRange(activities);
            await db.SaveChangesAsync(ct);

            var approvals = new List<WfActivityControl>();
            foreach (var activity in activities)
            {
                var approval = ActivityControl(process.RecId, activity.RecId, 6,
                    $"{activity.Code}_DECISION", "الاعتماد", 1, owner, true);
                var notes = ActivityControl(process.RecId, activity.RecId, 3,
                    $"{activity.Code}_NOTES", "ملاحظات", 2, owner, false);
                approvals.Add(approval);
                db.WfActivityControls.AddRange(approval, notes);
            }
            await db.SaveChangesAsync(ct);

            foreach (var approval in approvals)
            {
                db.WfActivityControlsOptions.AddRange(
                    ActivityOption(approval.RecId, "نعم", "نعم", 1, owner),
                    ActivityOption(approval.RecId, "لا", "لا", 2, owner));
                db.WfActivityControlsValidations.Add(ActivityValidation(approval, owner));
            }
            db.WfActivityMappingVariables.AddRange(approvals.Select((control, index) =>
                new WfActivityMappingVariable
                {
                    ActivityControlId = control.RecId,
                    VariableId = variables[index + 1].RecId,
                    VariableOrder = 1,
                    IsActive = true,
                    CreatedBy = owner,
                    OwnerAccountId = owner,
                }));
            db.WfTransitions.AddRange(activities.Select((activity, index) => new WfTransition
            {
                ProcessId = process.RecId,
                ActivityId = activity.RecId,
                VariableId = variables[index + 1].RecId,
                OperatorId = equalsOperator.RecId,
                Value = "لا",
                StepId = steps[6].RecId,
                SortOrder = (byte)(index + 1),
                IsActive = true,
                CreatedBy = owner,
                OwnerAccountId = owner,
            }));

                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }
}

