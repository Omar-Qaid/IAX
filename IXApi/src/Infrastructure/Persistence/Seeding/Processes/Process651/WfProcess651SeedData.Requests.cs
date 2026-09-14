using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Execution;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Variables;
using IAX.IXApi.Modules.Workflow.Processes;
using IAX.IXApi.Modules.Workflow.Activities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process651;

public sealed partial class WfProcess651SeedData
{
    private static async Task SeedRequestsAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var requestDetailsXml = "<Details><Control><ControlDataId>21731</ControlDataId><ControlLabel>Return type</ControlLabel><ControlLabelAR>نوع الإرجاع</ControlLabelAR><ControlValue>3</ControlValue><ControlId>6</ControlId><ExtendedProperties><Data><Item><ar>عملية عكسية خلال 15 يوم</ar><en>Reverse transaction within 15 days</en><value>3</value></Item></Data></ExtendedProperties><DisplayMember /><ValueMember /><UsedAsCriteria>False</UsedAsCriteria><UsedInSearch>False</UsedInSearch><ControlOrder>3</ControlOrder><RelatedObjectId>651</RelatedObjectId><ControlValueAR>عملية عكسية خلال 15 يوم</ControlValueAR><ControlValueEN>Reverse transaction within 15 days</ControlValueEN><Weight>0</Weight><TargetWeight>0.00</TargetWeight></Control></Details>";

        if (!await db.WfRequests.IgnoreQueryFilters().AnyAsync(x => x.RecId == 192663L, ct))
        {
            var request = new WfRequest
            {
                RecId = 192663L,
                EmployeeId = 157704L,
                ProcessId = 651L,
                RequestDate = new DateTime(2026, 9, 13, 18, 35, 41, 307),
                RequestDetails = requestDetailsXml,
                IsFinished = false,
                CreatedAt = new DateTime(2026, 9, 13, 18, 35, 41, 307),
                CreatedBy = "157704",
                IsStopped = false,
                Score = 0.00m,
                Progress = 0.00m
            };
            db.WfRequests.Add(request);
            await SaveWithIdentityAsync(db, "WfRequests", ct);
        }

        var requestDetails = new[]
        {
            new WfRequestDetail { RecId = 1947915L, RequestId = 192663L, ControlId = 6, ControlDataId = 21731L, Name = "Return type", NameAlias = "نوع الإرجاع", ControlValue = "3", ProcessId = 651L, SortOrder = 0, Score = 0 },
            new WfRequestDetail { RecId = 1947916L, RequestId = 192663L, ControlId = 2, ControlDataId = 21964L, Name = "Request Number", NameAlias = "رقم الطلب", ControlValue = "74004497", ProcessId = 651L, SortOrder = 1, Score = 0 },
            new WfRequestDetail { RecId = 1947917L, RequestId = 192663L, ControlId = 1, ControlDataId = 21069L, Name = "Invoice amount", NameAlias = "مبلغ الفاتورة", ControlValue = "495.00", ProcessId = 651L, SortOrder = 2, Score = 0 },
            new WfRequestDetail { RecId = 1947918L, RequestId = 192663L, ControlId = 1, ControlDataId = 21070L, Name = "Customer's mobile number", NameAlias = "رقم جوال العميل", ControlValue = "966508061437", ProcessId = 651L, SortOrder = 3, Score = 0 },
            new WfRequestDetail { RecId = 1947919L, RequestId = 192663L, ControlId = 10, ControlDataId = 21732L, Name = "If the client is outside Saudi Arabia, you must focus on:", NameAlias = "في حال كان العميل خارج السعودية يجب التركيز علي:-", ControlValue = null!, ProcessId = 651L, SortOrder = 4, Score = 0 },
            new WfRequestDetail { RecId = 1947920L, RequestId = 192663L, ControlId = 10, ControlDataId = 21733L, Name = "Customer's full name in English, soft code, account currency", NameAlias = "اسم العميل الثلاثي بالغة الانجليزية ، سوفت كود، عملة الحساب", ControlValue = null!, ProcessId = 651L, SortOrder = 5, Score = 0 },
            new WfRequestDetail { RecId = 1947921L, RequestId = 192663L, ControlId = 2, ControlDataId = 21079L, Name = "Customer name", NameAlias = "اسم العميل", ControlValue = "إسلام الوكيل", ProcessId = 651L, SortOrder = 6, Score = 0 },
            new WfRequestDetail { RecId = 1947922L, RequestId = 192663L, ControlId = 2, ControlDataId = 21080L, Name = "Bank name", NameAlias = "اسم البنك", ControlValue = null!, ProcessId = 651L, SortOrder = 6, Score = 0 },
            new WfRequestDetail { RecId = 1947923L, RequestId = 192663L, ControlId = 2, ControlDataId = 21963L, Name = "Nationality", NameAlias = "الجنسية", ControlValue = null!, ProcessId = 651L, SortOrder = 6, Score = 0 },
            new WfRequestDetail { RecId = 1947924L, RequestId = 192663L, ControlId = 1, ControlDataId = 21072L, Name = "bank account number", NameAlias = "رقم الحساب البنكي", ControlValue = "1", ProcessId = 651L, SortOrder = 7, Score = 0 },
            new WfRequestDetail { RecId = 1947925L, RequestId = 192663L, ControlId = 2, ControlDataId = 21071L, Name = "Account currency", NameAlias = "عملة الحساب", ControlValue = null!, ProcessId = 651L, SortOrder = 8, Score = 0 },
            new WfRequestDetail { RecId = 1947926L, RequestId = 192663L, ControlId = 2, ControlDataId = 21728L, Name = "Swift code", NameAlias = "سوفت كود", ControlValue = "1", ProcessId = 651L, SortOrder = 9, Score = 0 },
            new WfRequestDetail { RecId = 1947927L, RequestId = 192663L, ControlId = 3, ControlDataId = 21073L, Name = "Notes", NameAlias = "ملاحظات", ControlValue = null!, ProcessId = 651L, SortOrder = 12, Score = 0 },
        };
        foreach (var rd in requestDetails)
        {
            if (!await db.Set<WfRequestDetail>().IgnoreQueryFilters().AnyAsync(x => x.RecId == rd.RecId, ct))
                db.Set<WfRequestDetail>().Add(rd);
        }
        await SaveWithIdentityAsync(db, "WfRequestDetails", ct);

        var processVariables = new[]
        {
            new WfProcessVariable { RecId = 731043L, RequestId = 192663L, VariableId = 3618L, VariableValue = "3", SortOrder = 0 },
            new WfProcessVariable { RecId = 731044L, RequestId = 192663L, VariableId = 3616L, VariableValue = null, SortOrder = 1 },
            new WfProcessVariable { RecId = 731045L, RequestId = 192663L, VariableId = 3617L, VariableValue = null, SortOrder = 2 },
            new WfProcessVariable { RecId = 731046L, RequestId = 192663L, VariableId = 3630L, VariableValue = null, SortOrder = 4 },
        };
        foreach (var pv in processVariables)
        {
            if (!await db.Set<WfProcessVariable>().IgnoreQueryFilters().AnyAsync(x => x.RecId == pv.RecId, ct))
                db.Set<WfProcessVariable>().Add(pv);
        }
        await SaveWithIdentityAsync(db, "WfProcessVariables", ct);

        var assignments = new[]
        {
            new WfAssignment
            {
                RecId = 608023L,
                RequestId = 192663L,
                ActivityId = 6375L,
                UserId = 155725L,
                AssignDate = new DateTime(2026, 9, 13, 18, 35, 41, 743),
                IsFinished = true,
                AutoPassing = false,
                StepId = 16037L,
                Automatically = false,
                FinishedDate = new DateTime(2026, 9, 13, 18, 58, 0, 247),
                Transferred = false,
                Score = 14.58m
            },
            new WfAssignment
            {
                RecId = 608031L,
                RequestId = 192663L,
                ActivityId = 6376L,
                UserId = 155563L,
                AssignDate = new DateTime(2026, 9, 13, 18, 58, 0, 433),
                IsFinished = false,
                AutoPassing = false,
                StepId = 16038L,
                Automatically = false,
                Transferred = false,
                Score = 0.00m
            }
        };

        var validActivityIds = await db.WfActivities.IgnoreQueryFilters()
            .Select(x => x.RecId).ToHashSetAsync(ct);
        var validStepIds = await db.WfSteps.IgnoreQueryFilters()
            .Select(x => x.RecId).ToHashSetAsync(ct);

        foreach (var a in assignments)
        {
            if (validActivityIds.Contains(a.ActivityId)
                && validStepIds.Contains(a.StepId)
                && !await db.Set<WfAssignment>().IgnoreQueryFilters().AnyAsync(x => x.RecId == a.RecId, ct))
            {
                db.Set<WfAssignment>().Add(a);
            }
        }
        await SaveWithIdentityAsync(db, "WfAssignments", ct);

        var processDataList = new[]
        {
            new WfProcessData
            {
                RecId = 591307L,
                AssignmentID = 608023L,
                FinishDate = new DateTime(2026, 9, 13, 18, 58, 0, 240),
                ActivityDetails = "<Details><Control><ControlDataId>38452</ControlDataId><ControlLabel>Approval</ControlLabel><ControlLabelAR>الموافقة</ControlLabelAR><ControlValue>نعم</ControlValue><ControlId>6</ControlId><ExtendedProperties><Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item></Data></ExtendedProperties><DisplayMember /><ValueMember /><UsedAsCriteria>False</UsedAsCriteria><UsedInSearch>False</UsedInSearch><ControlOrder>1</ControlOrder><RelatedObjectId>0</RelatedObjectId><ControlValueAR>نعم</ControlValueAR><ControlValueEN>Yes</ControlValueEN><Weight>0</Weight><TargetWeight>0</TargetWeight></Control></Details>"
            }
        };

        var validAssignmentIds = await db.Set<WfAssignment>().IgnoreQueryFilters()
            .Select(x => x.RecId).ToHashSetAsync(ct);

        foreach (var pd in processDataList)
        {
            if (pd.AssignmentID.HasValue
                && validAssignmentIds.Contains(pd.AssignmentID.Value)
                && !await db.Set<WfProcessData>().IgnoreQueryFilters().AnyAsync(x => x.RecId == pd.RecId, ct))
            {
                db.Set<WfProcessData>().Add(pd);
            }
        }
        await SaveWithIdentityAsync(db, "WfProcessData", ct);

        var activityDetails = new[]
        {
            new WfActivityDetail { RecId = 1202217L, ProcessId = 591307L, AssignmentID = 608023L, ControlId = 6, ControlDataId = 38452L, Name = "Approval", NameAlias = "الموافقة", ControlValue = "نعم", Value = "Yes", ValueAlias = "نعم", SortOrder = 1 },
            new WfActivityDetail { RecId = 1202218L, ProcessId = 591307L, AssignmentID = 608023L, ControlId = 3, ControlDataId = 38453L, Name = "Notes", NameAlias = "ملاحظات", ControlValue = null, Value = null, ValueAlias = null, SortOrder = 2 },
        };

        foreach (var detail in activityDetails)
        {
            if (validAssignmentIds.Contains(detail.AssignmentID)
                && !await db.Set<WfActivityDetail>().IgnoreQueryFilters()
                    .AnyAsync(x => x.RecId == detail.RecId, ct))
            {
                db.Set<WfActivityDetail>().Add(detail);
            }
        }

        await db.Database.OpenConnectionAsync(ct);
        try
        {
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[WfActivityDetails] ON", ct);
            await db.SaveChangesAsync(ct);
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[WfActivityDetails] OFF", ct);
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}

