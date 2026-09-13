using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Variables;
using IAX.IXApi.Modules.Workflow.Processes;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process603;

public sealed partial class WfProcess603SeedData
{
    private static async Task SeedRequestsAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingRequestId = await db.WfRequests.IgnoreQueryFilters()
            .Where(x => x.RecId == 129252L)
            .Select(x => x.RecId)
            .FirstOrDefaultAsync(ct);

        if (existingRequestId != 0) return;

        var requestDetailsXml = "<Details><Control><ControlDataId>20627</ControlDataId><ControlLabel>Name of the depository employee</ControlLabel><ControlLabelAR>اسم موظف الايداع</ControlLabelAR><ControlValue>155706</ControlValue><ControlId>12</ControlId><ExtendedProperties /><DisplayMember /><ValueMember /><UsedAsCriteria>True</UsedAsCriteria><UsedInSearch>False</UsedInSearch><ControlOrder>1</ControlOrder><RelatedObjectId>603</RelatedObjectId><ControlValueAR>هيا حمدان سعد الدوسري</ControlValueAR><ControlValueEN>Haya Hamdan Saad Aldawsari</ControlValueEN><Weight>0</Weight><TargetWeight>0.00</TargetWeight></Control><Control><ControlDataId>20628</ControlDataId><ControlLabel>رقم الايداع</ControlLabel><ControlLabelAR>رقم الايداع</ControlLabelAR><ControlValue>1</ControlValue><ControlId>1</ControlId><ExtendedProperties /><DisplayMember /><ValueMember /><UsedAsCriteria>True</UsedAsCriteria><UsedInSearch>False</UsedInSearch><ControlOrder>2</ControlOrder><RelatedObjectId>603</RelatedObjectId><ControlValueAR /><ControlValueEN /><Weight>0</Weight><TargetWeight>0.00</TargetWeight></Control><Control><ControlDataId>20629</ControlDataId><ControlLabel>Amount</ControlLabel><ControlLabelAR>المبلغ</ControlLabelAR><ControlValue>4175</ControlValue><ControlId>1</ControlId><ExtendedProperties /><DisplayMember /><ValueMember /><UsedAsCriteria>True</UsedAsCriteria><UsedInSearch>False</UsedInSearch><ControlOrder>3</ControlOrder><RelatedObjectId>603</RelatedObjectId><ControlValueAR /><ControlValueEN /><Weight>0</Weight><TargetWeight>0.00</TargetWeight></Control><Control><ControlDataId>20654</ControlDataId><ControlLabel>Filing date</ControlLabel><ControlLabelAR>تاريخ الايداع</ControlLabelAR><ControlValue>2026-04-06</ControlValue><ControlId>4</ControlId><ExtendedProperties /><DisplayMember /><ValueMember /><UsedAsCriteria>True</UsedAsCriteria><UsedInSearch>False</UsedInSearch><ControlOrder>4</ControlOrder><RelatedObjectId>603</RelatedObjectId><ControlValueAR /><ControlValueEN /><Weight>0</Weight><TargetWeight>0.00</TargetWeight></Control><Control><ControlDataId>20655</ControlDataId><ControlLabel>Deposit period from</ControlLabel><ControlLabelAR>فترة الايداع من</ControlLabelAR><ControlValue>2026-04-01</ControlValue><ControlId>4</ControlId><ExtendedProperties /><DisplayMember /><ValueMember /><UsedAsCriteria>True</UsedAsCriteria><UsedInSearch>False</UsedInSearch><ControlOrder>5</ControlOrder><RelatedObjectId>603</RelatedObjectId><ControlValueAR /><ControlValueEN /><Weight>0</Weight><TargetWeight>0.00</TargetWeight></Control><Control><ControlDataId>20656</ControlDataId><ControlLabel>Deposit period to</ControlLabel><ControlLabelAR>فترة الايداع الى</ControlLabelAR><ControlValue>2026-04-04</ControlValue><ControlId>4</ControlId><ExtendedProperties /><DisplayMember /><ValueMember /><UsedAsCriteria>True</UsedAsCriteria><UsedInSearch>False</UsedInSearch><ControlOrder>6</ControlOrder><RelatedObjectId>603</RelatedObjectId><ControlValueAR /><ControlValueEN /><Weight>0</Weight><TargetWeight>0.00</TargetWeight></Control><Control><ControlDataId>20634</ControlDataId><ControlLabel>Has the transfer document been attached?</ControlLabel><ControlLabelAR>هل تم ارفاق سند التحويل</ControlLabelAR><ControlValue>نعم</ControlValue><ControlId>6</ControlId><ExtendedProperties><Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value><weight>0</weight></Item><Item><ar>لا</ar><en>No</en><value>لا</value><weight>0</weight></Item></Data></ExtendedProperties><DisplayMember /><ValueMember /><UsedAsCriteria>False</UsedAsCriteria><UsedInSearch>False</UsedInSearch><ControlOrder>7</ControlOrder><RelatedObjectId>603</RelatedObjectId><ControlValueAR>نعم</ControlValueAR><ControlValueEN>Yes</ControlValueEN><Weight>0.00</Weight><TargetWeight>0.00</TargetWeight></Control><Control><ControlDataId>21962</ControlDataId><ControlLabel>Country</ControlLabel><ControlLabelAR>الدولة</ControlLabelAR><ControlValue>ksa</ControlValue><ControlId>6</ControlId><ExtendedProperties><Data><Item><ar>المملكة العربية السعودية</ar><en>Ksa</en><value>ksa</value><weight>0</weight></Item><Item><ar>الإمارات العربية المتحدة</ar><en>Uae</en><value>uae</value><weight>0</weight></Item><Item><ar>عمان</ar><en>Oman</en><value>om</value><weight>0</weight></Item><Item><ar>قطر</ar><en>Qatar</en><value>qat</value><weight>0</weight></Item><Item><ar>الكويت</ar><en>Kuwait</en><value>kw</value><weight>0</weight></Item><Item><ar>البحرين</ar><en>Bahrain</en><value>bh</value><weight>0</weight></Item><Item><ar>الولايات المتحدة الأمريكية </ar><en>Usa</en><value>usa</value><weight>0</weight></Item><Item><ar>مصر</ar><en>Eygpt</en><value>eg</value><weight>0</weight></Item></Data></ExtendedProperties><DisplayMember /><ValueMember /><UsedAsCriteria>True</UsedAsCriteria><UsedInSearch>False</UsedInSearch><ControlOrder>8</ControlOrder><RelatedObjectId>603</RelatedObjectId><ControlValueAR>المملكة العربية السعودية</ControlValueAR><ControlValueEN>Ksa</ControlValueEN><Weight>0.00</Weight><TargetWeight>0.00</TargetWeight></Control><Control><ControlDataId>20633</ControlDataId><ControlLabel>NOTES</ControlLabel><ControlLabelAR>ملاحظات</ControlLabelAR><ControlValue /><ControlId>3</ControlId><ExtendedProperties /><DisplayMember /><ValueMember /><UsedAsCriteria>False</UsedAsCriteria><UsedInSearch>False</UsedInSearch><ControlOrder>10</ControlOrder><RelatedObjectId>603</RelatedObjectId><ControlValueAR /><ControlValueEN /><Weight>0</Weight><TargetWeight>0.00</TargetWeight></Control></Details>";

        if (!await db.WfRequests.IgnoreQueryFilters().AnyAsync(x => x.RecId == 129252L, ct))
        {
            var request = new WfRequest
            {
                RecId = 129252L,
                EmployeeId = 156144L,
                ProcessId = 603L,
                RequestDate = new DateTime(2026, 4, 6, 23, 35, 46, 213),
                RequestDetails = requestDetailsXml,
                IsFinished = true,
                CreatedAt = new DateTime(2026, 4, 6, 23, 35, 46, 213),
                CreatedBy = "156144",
                FinishedDate = new DateTime(2026, 4, 8, 18, 44, 12, 170)
            };
            db.WfRequests.Add(request);
            await SaveWithIdentityAsync(db, "WfRequests", ct);
        }

        var processVariables = new[]
        {
            new WfProcessVariable { RecId = 485825L, RequestId = 129252L, VariableId = 3502L, VariableValue = "نعم", SortOrder = 1 },
            new WfProcessVariable { RecId = 485826L, RequestId = 129252L, VariableId = 3503L, VariableValue = null, SortOrder = 2 },
            new WfProcessVariable { RecId = 485827L, RequestId = 129252L, VariableId = 3504L, VariableValue = "ksa", SortOrder = 3 },
            new WfProcessVariable { RecId = 485828L, RequestId = 129252L, VariableId = 3554L, VariableValue = null, SortOrder = 4 },
            new WfProcessVariable { RecId = 485829L, RequestId = 129252L, VariableId = 3555L, VariableValue = null, SortOrder = 5 },
            new WfProcessVariable { RecId = 485830L, RequestId = 129252L, VariableId = 3720L, VariableValue = null, SortOrder = 6 },
            new WfProcessVariable { RecId = 485831L, RequestId = 129252L, VariableId = 3577L, VariableValue = null, SortOrder = 7 },
            new WfProcessVariable { RecId = 485832L, RequestId = 129252L, VariableId = 3744L, VariableValue = null, SortOrder = 8 },
        };
        foreach (var pv in processVariables)
        {
            if (!await db.Set<WfProcessVariable>().IgnoreQueryFilters().AnyAsync(x => x.RecId == pv.RecId, ct))
                db.Set<WfProcessVariable>().Add(pv);
        }
        await SaveWithIdentityAsync(db, "WfProcessVariables", ct);

        var assignment = new IAX.IXApi.Modules.Workflow.Execution.WfAssignment
        {
            RecId = 437824L,
            RequestId = 129252L,
            ActivityId = 6140L,
            UserId = 155568L,
            AssignDate = new DateTime(2026, 4, 6, 23, 35, 46, 563),
            IsFinished = true,
            AutoPassing = false,
            StepId = 15794L,
            Automatically = false,
            FinishedDate = new DateTime(2026, 4, 8, 18, 44, 12, 7),
            Transferred = false,
            Score = 0
        };
        if (!await db.Set<IAX.IXApi.Modules.Workflow.Execution.WfAssignment>().IgnoreQueryFilters().AnyAsync(x => x.RecId == assignment.RecId, ct))
        {
            db.Set<IAX.IXApi.Modules.Workflow.Execution.WfAssignment>().Add(assignment);
            await SaveWithIdentityAsync(db, "WfAssignments", ct);
        }

        var processData = new WfProcessData
        {
            RecId = 431692L,
            AssignmentID = 437824L,
            FinishDate = new DateTime(2026, 4, 8, 18, 44, 12, 3),
            ActivityDetails = "<Details><Control><ControlDataId>37894</ControlDataId><ControlLabel>Do the deposits match</ControlLabel><ControlLabelAR>هل الايداعات مطابقة</ControlLabelAR><ControlValue>نعم</ControlValue><ControlId>6</ControlId><ExtendedProperties><Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item><Item><ar>تمرير لمحاسب دول الخليج</ar><en>Pass to the accountant of the Gulf countries</en><value>تمرير لمحاسب دول الخليج</value></Item></Data></ExtendedProperties><DisplayMember /><ValueMember /><UsedAsCriteria>False</UsedAsCriteria><UsedInSearch>False</UsedInSearch><ControlOrder>1</ControlOrder><RelatedObjectId>0</RelatedObjectId><ControlValueAR>نعم</ControlValueAR><ControlValueEN>Yes</ControlValueEN><Weight>0</Weight><TargetWeight>0</TargetWeight></Control><Control><ControlDataId>37895</ControlDataId><ControlLabel>NOTES</ControlLabel><ControlLabelAR>ملاحظات</ControlLabelAR><ControlValue /><ControlId>3</ControlId><ExtendedProperties /><DisplayMember /><ValueMember /><UsedAsCriteria>False</UsedAsCriteria><UsedInSearch>False</UsedInSearch><ControlOrder>2</ControlOrder><RelatedObjectId>0</RelatedObjectId><ControlValueAR /><ControlValueEN /><Weight>0</Weight><TargetWeight>0</TargetWeight></Control></Details>"
        };
        if (!await db.Set<WfProcessData>().IgnoreQueryFilters().AnyAsync(x => x.RecId == processData.RecId, ct))
        {
            db.Set<WfProcessData>().Add(processData);
            await SaveWithIdentityAsync(db, "WfProcessData", ct);
        }
    }
}
