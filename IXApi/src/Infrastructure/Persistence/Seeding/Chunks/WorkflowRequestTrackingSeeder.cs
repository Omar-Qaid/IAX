using System.Xml.Linq;
using System.Text.Json;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Departments;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Organization.Genders;
using IAX.IXApi.Modules.Organization.Nationalities;
using IAX.IXApi.Modules.Organization.Occupations;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Categories;
using IAX.IXApi.Modules.Workflow.Controls;
using IAX.IXApi.Modules.Workflow.Execution;
using IAX.IXApi.Modules.Workflow.Performers;
using IAX.IXApi.Modules.Workflow.Priorities;
using IAX.IXApi.Modules.Workflow.Processes;
using IAX.IXApi.Modules.Workflow.ProcessTypes;
using IAX.IXApi.Modules.Workflow.PrintTemplates;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Steps;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>
/// Seeds the complete legacy execution for request 94037 and guarantees that every
/// workflow process has a representative request and a published print template.
/// </summary>
public sealed class WorkflowRequestTrackingSeeder : ISeeder
{
    private const long ProcessId = 662;
 
    private const string Signature = "data:image/svg+xml;base64,PHN2Zy8+";

    private sealed record RequestControl(long Id, byte Type, string Code, string Label, string LabelAr, byte Order, bool Mandatory = false, bool Active = true, string? Properties = null);
    private sealed record ActivityControl(long Id, long Activity, byte Type, string Code, string Label, string LabelAr, byte Order, bool Mandatory, string? Properties = null);
    private sealed record Request(long Id, DateTime Date, DateTime Finished, string CreatedBy);
    private sealed record RequestValue(long Id, long Request, byte Type, long Control, string Label, string LabelAr, string Value, bool Criteria, byte Order, string ValueAr = "", string ValueEn = "");
    private sealed record Assignment(long Id, long Request, long Activity, long User, DateTime Assigned, DateTime Finished, long Step, bool AutoPassing, byte Hours, bool Automatically);
    private sealed record Task(long Id, long Assignment, DateTime Finished);
    private sealed record ActivityValue(long Id, long Task, long Assignment, byte Type, long Control, string Label, string LabelAr, string Value, byte Order, string ValueAr, string ValueEn);
    private sealed record Employee(long Id, string Code, string Name, string NameAr, short Department, short Occupation, byte Gender, short Nationality, DateTime Created);

    private static readonly Request[] Requests =
    [
        new(94037, new(2025, 12, 15, 0, 3, 58, 290), new(2025, 12, 15, 10, 19, 10, 840), "157424"),
    ];

    private static readonly RequestValue[] Values =
    [
        new(685037, 94037, 1, 21201, "Total sales", "إجمالي المبيعات:-", "5239", false, 0),
        new(685038, 94037, 1, 21202, "cash", "نقدي", "2206", false, 1),
        new(685039, 94037, 1, 21204, "Mada", "مدى", "1753", false, 2),
        new(685040, 94037, 1, 21205, "Visa", "فيزا", "762", false, 3),
        new(685041, 94037, 1, 21206, "MasterCard", "ماستر كارد", "69", false, 4),
        new(685042, 94037, 1, 21207, "American Express", "امريكان اكسبرس", "0", false, 5),
        new(685043, 94037, 1, 21208, "Gulf Network", "شبكة خليجية", "254", false, 6),
        new(685044, 94037, 1, 21209, "Tabby", "تابي", "195", false, 7),
        new(685045, 94037, 1, 21210, "Tamara", "تمارا", "0", false, 8),
        new(685046, 94037, 4, 21162, "Today's closing date", "تاريخ اقفال اليوم", "2025-12-14", true, 9),
        new(685047, 94037, 12, 21166, "Locksmith Name Today", "اسم موظف الاقفال اليوم", "157430", true, 10, "فهد مشعل الشمري", "فهد مشعل الشمري"),
        new(685048, 94037, 20, 21167, "signature", "التوقيع", Signature, false, 11),
        new(685049, 94037, 10, 21172, "The following must be attached:", "يجب ارفاق الاتي:-", "", false, 12),
        new(685050, 94037, 10, 21173, "Budget image", "صور واضحة للموازنة", "", false, 13),
        new(685051, 94037, 10, 21174, "Image of network receipts", "صورة ايصالات الشبكة", "", false, 14),
        new(685052, 94037, 3, 21168, "note", "ملاحظة", "", false, 15),
    ];

    private static readonly Assignment[] Assignments =
    [
        new(321331, 94037, 6461, 155350, new(2025, 12, 15, 0, 3, 58, 350), new(2025, 12, 15, 9, 18, 18, 560), 16103, false, 0, false),
        new(321463, 94037, 6452, 157077, new(2025, 12, 15, 9, 18, 18, 623), new(2025, 12, 15, 10, 19, 10, 797), 16098, true, 1, true),
    ];

    private static readonly Task[] Tasks =
    [
        new(313179, 321331, new(2025, 12, 15, 9, 18, 18, 557)),
        new(313322, 321463, new(2025, 12, 15, 10, 19, 10, 797)),
    ];

    private static readonly ActivityValue[] ActivityValues =
    [
        new(383793, 313179, 321331, 6, 38712, "Are the locks compatible?", "هل الاقفال مطابق", "YES", 1, "نعم", "YES"),
        new(383794, 313179, 321331, 3, 38713, "note", "ملاحظة", "", 2, "", ""),
        new(384135, 313322, 321463, 10, 0, "The transaction was processed.", "تم تمرير المعاملة", "تم تمرير المعاملة آلياً", 0, "تم تمرير المعاملة آلياً", "The transaction was processed automatically."),
    ];

    public async System.Threading.Tasks.Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
    {
        _ = roles;
        var createdBy = (await users.FindByNameAsync("sys"))?.Id ?? "sys";
 
        await SeedExecutionAsync(db, createdBy, ct);
        await SeedRequestForEveryProcessAsync(db, createdBy, ct);
        await SeedPrintTemplateAsync(db, createdBy, ct);
        await SeedPrintTemplateForEveryProcessAsync(db, createdBy, ct);
    }

   

    private static async System.Threading.Tasks.Task SeedExecutionAsync(ApplicationDbContext db, string by, CancellationToken ct)
    {
        const long requestId = 94037;

        var details = Values
            .Where(value => value.Request == requestId)
            .Select(value => new WfRequestDetail
            {
                RecId = value.Id,
                ProcessId = ProcessId,
                RequestId = value.Request,
                ControlId = value.Type,
                ControlDataId = value.Control,
                ControlLabel = value.Label,
                ControlLabelAR = value.LabelAr,
                ControlValue = value.Value,
                ControlValueAR = value.ValueAr,
                ControlValueEN = value.ValueEn,
                UsedAsCriteria = value.Criteria,
                SortOrder = value.Order,
                CreatedBy = by,
                OwnerAccountId = by,
            })
            .ToList();

        foreach (var request in Requests.Where(request => request.Id == requestId))
        {
            if (await db.WfRequests.IgnoreQueryFilters().AnyAsync(row => row.RecId == request.Id, ct))
                continue;

            db.WfRequests.Add(new WfRequest
            {
                RecId = request.Id,
                Code = request.Id.ToString(),
                Name = $"Daily fund closing - {request.Date:yyyy-MM-dd}",
                ProcessId = ProcessId,
                EmployeeId = 157424,
                RequestDate = request.Date,
                RequestDetails = BuildXml(details.Where(detail => detail.RequestId == request.Id)),
                IsFinished = true,
                FinishedDate = request.Finished,
                Progress = 100,
                IsActive = true,
                CreatedAt = request.Date,
                CreatedBy = request.CreatedBy,
                OwnerAccountId = by,
            });
            await SaveIdentityAsync(db, "WfRequests", ct);
        }

        foreach (var d in details)
            if (!await db.WfRequestDetails.IgnoreQueryFilters().AnyAsync(x=>x.RecId==d.RecId,ct)) { db.WfRequestDetails.Add(d); await SaveIdentityAsync(db,"WfRequestDetails",ct); }
        foreach (var a in Assignments.Where(x=>x.Request==requestId))
            if (!await db.WfAssignments.IgnoreQueryFilters().AnyAsync(x=>x.RecId==a.Id,ct)) { db.WfAssignments.Add(new WfAssignment {RecId=a.Id,RequestId=a.Request,ActivityId=a.Activity,UserId=a.User,AssignDate=a.Assigned,IsFinished=true,FinishedDate=a.Finished,AutoPassing=a.AutoPassing,AutoPassingHrs=a.Hours,StepId=a.Step,Automatically=a.Automatically,CreatedBy=by,OwnerAccountId=by}); await SaveIdentityAsync(db,"WfAssignments",ct); }
        var selectedAssignmentIds=Assignments.Where(x=>x.Request==requestId).Select(x=>x.Id).ToHashSet();
        var activityDetails=ActivityValues.Where(v=>selectedAssignmentIds.Contains(v.Assignment)).Select(v=>new WfActivityDetail {RecId=v.Id,ProcessId=v.Task,AssignmentID=v.Assignment,ControlId=v.Type,ControlDataId=v.Control,ControlLabel=v.Label,ControlLabelAR=v.LabelAr,ControlValue=v.Value,ControlValueAR=v.ValueAr,ControlValueEN=v.ValueEn,SortOrder=v.Order,CreatedBy=by,OwnerAccountId=by}).ToList();
        foreach (var t in Tasks.Where(x=>selectedAssignmentIds.Contains(x.Assignment)))
            if (!await db.WfProcessData.IgnoreQueryFilters().AnyAsync(x=>x.RecId==t.Id,ct)) { db.WfProcessData.Add(new WfProcessData {RecId=t.Id,AssignmentID=t.Assignment,FinishDate=t.Finished,ActivityDetails=BuildXml(activityDetails.Where(x=>x.ProcessId==t.Id)),CreatedBy=by,OwnerAccountId=by}); await SaveIdentityAsync(db,"WfProcessData",ct); }
        foreach (var d in activityDetails)
            if (!await db.WfActivityDetails.IgnoreQueryFilters().AnyAsync(x=>x.RecId==d.RecId,ct)) { db.WfActivityDetails.Add(d); await SaveIdentityAsync(db,"WfActivityDetails",ct); }
    }

    private static async System.Threading.Tasks.Task SeedPrintTemplateAsync(
        ApplicationDbContext db,
        string by,
        CancellationToken ct)
    {
        const long requestId = 94037;
        const string templateCode = "DAILY_FUND_CLOSING";

        var template = await db.WfPrintTemplates
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                row => row.ProcessId == ProcessId && row.Code == templateCode,
                ct);

        if (template is null)
        {
            var processHasDefaultTemplate = await db.WfPrintTemplates
                .IgnoreQueryFilters()
                .AnyAsync(
                    row => row.ProcessId == ProcessId && row.IsDefault && row.IsActive && !row.IsDeleted,
                    ct);

            template = new WfPrintTemplate
            {
                ProcessId = ProcessId,
                Code = templateCode,
                Name = "Daily fund closing printout",
                Description = "Default A4 printout for the daily fund closing workflow.",
                PageSize = "A4",
                Orientation = "portrait",
                Language = "en",
                IsDefault = !processHasDefaultTemplate,
                Status = WfPrintTemplateStatus.Published,
                IsActive = true,
                CreatedBy = by,
                OwnerAccountId = by,
            };
            db.WfPrintTemplates.Add(template);
            await db.SaveChangesAsync(ct);
        }

        var version = await db.WfPrintTemplateVersions
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                row => row.TemplateId == template.RecId && row.VersionNo == 1,
                ct);

        if (version is null)
        {
            version = new WfPrintTemplateVersion
            {
                TemplateId = template.RecId,
                VersionNo = 1,
                TemplateJson = BuildPrintTemplateJson(),
                IsPublished = true,
                PublishedBy = by,
                PublishedAt = DateTime.UtcNow,
                CreatedBy = by,
                OwnerAccountId = by,
            };
            db.WfPrintTemplateVersions.Add(version);
            await db.SaveChangesAsync(ct);
        }

        if (template.CurrentVersionId is null)
        {
            template.CurrentVersionId = version.RecId;
            template.Status = WfPrintTemplateStatus.Published;
            await db.SaveChangesAsync(ct);
        }

        var requestVersionExists = await db.WfRequestPrintVersions
            .IgnoreQueryFilters()
            .AnyAsync(
                row => row.RequestId == requestId && row.TemplateId == template.RecId,
                ct);

        if (!requestVersionExists)
        {
            db.WfRequestPrintVersions.Add(new WfRequestPrintVersion
            {
                RequestId = requestId,
                TemplateId = template.RecId,
                TemplateVersionId = version.RecId,
                SelectedAt = DateTime.UtcNow,
                SelectedBy = by,
                CreatedBy = by,
                OwnerAccountId = by,
            });
            await db.SaveChangesAsync(ct);
        }
    }

    private static async System.Threading.Tasks.Task SeedRequestForEveryProcessAsync(
        ApplicationDbContext db,
        string by,
        CancellationToken ct)
    {
        var processes = await db.WfProcesses
            .IgnoreQueryFilters()
            .Where(process => process.IsActive && !process.IsDeleted)
            .OrderBy(process => process.RecId)
            .Select(process => new { process.RecId, process.Name })
            .ToListAsync(ct);

        var processIdsWithRequests = await db.WfRequests
            .IgnoreQueryFilters()
            .Where(request => !request.IsDeleted)
            .Select(request => request.ProcessId)
            .Distinct()
            .ToListAsync(ct);
        var existingProcessIds = processIdsWithRequests.ToHashSet();
        var seededAt = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);

        foreach (var process in processes.Where(process => !existingProcessIds.Contains(process.RecId)))
        {
            var requestDate = seededAt.AddMinutes(process.RecId % 1440);
            var processName = string.IsNullOrWhiteSpace(process.Name)
                ? $"Process {process.RecId}"
                : process.Name;
            db.WfRequests.Add(new WfRequest
            {
                Code = $"SEED-{process.RecId}",
                Name = $"{processName} sample request",
                ProcessId = process.RecId,
                EmployeeId = null,
                RequestDate = requestDate,
                RequestDetails = new XElement("Details").ToString(SaveOptions.DisableFormatting),
                IsFinished = false,
                Progress = 0,
                IsActive = true,
                CreatedAt = requestDate,
                CreatedBy = by,
                OwnerAccountId = by,
            });
        }

        await db.SaveChangesAsync(ct);
    }

    private static async System.Threading.Tasks.Task SeedPrintTemplateForEveryProcessAsync(
        ApplicationDbContext db,
        string by,
        CancellationToken ct)
    {
        var processes = await db.WfProcesses
            .IgnoreQueryFilters()
            .Where(process => process.IsActive && !process.IsDeleted && process.RecId != ProcessId)
            .OrderBy(process => process.RecId)
            .Select(process => new { process.RecId, process.Name })
            .ToListAsync(ct);

        foreach (var process in processes)
        {
            var processName = string.IsNullOrWhiteSpace(process.Name)
                ? $"Process {process.RecId}"
                : process.Name;
            var templateCode = $"PROCESS_{process.RecId}_PRINTOUT";
            var template = await db.WfPrintTemplates
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    row => row.ProcessId == process.RecId && row.Code == templateCode,
                    ct);

            if (template is null)
            {
                var processHasDefaultTemplate = await db.WfPrintTemplates
                    .IgnoreQueryFilters()
                    .AnyAsync(
                        row => row.ProcessId == process.RecId
                            && row.IsDefault
                            && row.IsActive
                            && !row.IsDeleted,
                        ct);

                template = new WfPrintTemplate
                {
                    ProcessId = process.RecId,
                    Code = templateCode,
                    Name = $"{processName} printout",
                    Description = $"Seeded A4 printout for the {processName} workflow.",
                    PageSize = "A4",
                    Orientation = "portrait",
                    Language = "en",
                    IsDefault = !processHasDefaultTemplate,
                    Status = WfPrintTemplateStatus.Published,
                    IsActive = true,
                    CreatedBy = by,
                    OwnerAccountId = by,
                };
                db.WfPrintTemplates.Add(template);
                await db.SaveChangesAsync(ct);
            }

            var version = await db.WfPrintTemplateVersions
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    row => row.TemplateId == template.RecId && row.VersionNo == 1,
                    ct);

            if (version is null)
            {
                version = new WfPrintTemplateVersion
                {
                    TemplateId = template.RecId,
                    VersionNo = 1,
                    TemplateJson = BuildGenericPrintTemplateJson(processName),
                    IsPublished = true,
                    PublishedBy = by,
                    PublishedAt = DateTime.UtcNow,
                    CreatedBy = by,
                    OwnerAccountId = by,
                };
                db.WfPrintTemplateVersions.Add(version);
                await db.SaveChangesAsync(ct);
            }

            if (template.CurrentVersionId != version.RecId
                || template.Status != WfPrintTemplateStatus.Published)
            {
                template.CurrentVersionId = version.RecId;
                template.Status = WfPrintTemplateStatus.Published;
                await db.SaveChangesAsync(ct);
            }

            var requestId = await db.WfRequests
                .IgnoreQueryFilters()
                .Where(request => request.ProcessId == process.RecId && !request.IsDeleted)
                .OrderBy(request => request.RecId)
                .Select(request => (long?)request.RecId)
                .FirstOrDefaultAsync(ct);

            if (requestId is null)
                continue;

            var requestVersionExists = await db.WfRequestPrintVersions
                .IgnoreQueryFilters()
                .AnyAsync(
                    row => row.RequestId == requestId.Value && row.TemplateId == template.RecId,
                    ct);

            if (!requestVersionExists)
            {
                db.WfRequestPrintVersions.Add(new WfRequestPrintVersion
                {
                    RequestId = requestId.Value,
                    TemplateId = template.RecId,
                    TemplateVersionId = version.RecId,
                    SelectedAt = DateTime.UtcNow,
                    SelectedBy = by,
                    CreatedBy = by,
                    OwnerAccountId = by,
                });
                await db.SaveChangesAsync(ct);
            }
        }
    }

    private static string BuildPrintTemplateJson()
    {
        var document = new PrintTemplateDocument
        {
            SchemaVersion = 1,
            Language = "en",
            Direction = "ltr",
            Page = new PrintTemplatePage
            {
                Size = "A4",
                Orientation = "portrait",
                Margins = new PrintTemplateMargins
                {
                    Top = 15,
                    Right = 15,
                    Bottom = 15,
                    Left = 15,
                },
            },
            Header =
            [
                new PrintImageElement
                {
                    Id = "company-logo",
                    SourceType = "companyLogo",
                    AltText = "Company logo",
                },
                new PrintFieldElement
                {
                    Id = "company-name",
                    Label = "Company",
                    Binding = new PrintFieldBinding { SourceType = "company", Source = "name" },
                },
                new PrintTextElement
                {
                    Id = "document-title",
                    Value = "Daily fund closing",
                    Style = new PrintElementStyle { FontSize = 18, FontWeight = 700, Alignment = "center" },
                },
            ],
            Sections =
            [
                new PrintSectionElement
                {
                    Id = "request-information",
                    Title = "Request information",
                    Columns = 2,
                    Elements =
                    [
                        new PrintFieldElement
                        {
                            Id = "request-number",
                            Label = "Request",
                            Binding = new PrintFieldBinding { SourceType = "system", Source = "requestNumber" },
                        },
                        new PrintFieldElement
                        {
                            Id = "request-date",
                            Label = "Request date",
                            Binding = new PrintFieldBinding { SourceType = "system", Source = "requestDate" },
                            Format = new PrintValueFormat { Type = "date", Pattern = "yyyy-MM-dd" },
                        },
                        new PrintFieldElement
                        {
                            Id = "total-sales",
                            Label = "Total sales",
                            Binding = new PrintFieldBinding { SourceType = "requestControl", RequestControlId = 21201 },
                            Format = new PrintValueFormat { Type = "number" },
                        },
                        new PrintFieldElement
                        {
                            Id = "closing-date",
                            Label = "Closing date",
                            Binding = new PrintFieldBinding { SourceType = "requestControl", RequestControlId = 21162 },
                            Format = new PrintValueFormat { Type = "date", Pattern = "yyyy-MM-dd" },
                        },
                    ],
                },
            ],
            Footer =
            [
                new PrintDateElement { Id = "print-date" },
                new PrintPageNumberElement { Id = "page-number" },
            ],
            MissingFieldBehavior = "empty",
        };

        return JsonSerializer.Serialize(document, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private static string BuildGenericPrintTemplateJson(string processName)
    {
        var document = new PrintTemplateDocument
        {
            SchemaVersion = 1,
            Language = "en",
            Direction = "ltr",
            Page = new PrintTemplatePage
            {
                Size = "A4",
                Orientation = "portrait",
                Margins = new PrintTemplateMargins { Top = 15, Right = 15, Bottom = 15, Left = 15 },
            },
            Header =
            [
                new PrintImageElement
                {
                    Id = "company-logo",
                    SourceType = "companyLogo",
                    AltText = "Company logo",
                },
                new PrintTextElement
                {
                    Id = "document-title",
                    Value = processName,
                    Style = new PrintElementStyle { FontSize = 18, FontWeight = 700, Alignment = "center" },
                },
            ],
            Sections =
            [
                new PrintSectionElement
                {
                    Id = "request-information",
                    Title = "Request information",
                    Columns = 2,
                    Elements =
                    [
                        new PrintFieldElement
                        {
                            Id = "request-number",
                            Label = "Request",
                            Binding = new PrintFieldBinding { SourceType = "system", Source = "requestNumber" },
                        },
                        new PrintFieldElement
                        {
                            Id = "request-date",
                            Label = "Request date",
                            Binding = new PrintFieldBinding { SourceType = "system", Source = "requestDate" },
                            Format = new PrintValueFormat { Type = "date", Pattern = "yyyy-MM-dd" },
                        },
                        new PrintFieldElement
                        {
                            Id = "request-status",
                            Label = "Status",
                            Binding = new PrintFieldBinding { SourceType = "system", Source = "requestStatus" },
                        },
                        new PrintFieldElement
                        {
                            Id = "requested-by",
                            Label = "Requested by",
                            Binding = new PrintFieldBinding { SourceType = "system", Source = "submittedBy" },
                        },
                    ],
                },
            ],
            Footer =
            [
                new PrintDateElement { Id = "print-date" },
                new PrintPageNumberElement { Id = "page-number" },
            ],
            MissingFieldBehavior = "empty",
        };

        return JsonSerializer.Serialize(document, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private static string BuildXml(IEnumerable<WfRequestDetail> rows) => new XElement("Details",rows.OrderBy(x=>x.SortOrder).Select(x=>new XElement("Control",new XElement("ControlDataId",x.ControlDataId),new XElement("ControlLabel",x.ControlLabel),new XElement("ControlLabelAR",x.ControlLabelAR),new XElement("ControlValue",x.ControlValue),new XElement("ControlId",x.ControlId),new XElement("UsedAsCriteria",x.UsedAsCriteria),new XElement("ControlOrder",x.SortOrder),new XElement("RelatedObjectId",ProcessId),new XElement("ControlValueAR",x.ControlValueAR),new XElement("ControlValueEN",x.ControlValueEN)))).ToString(SaveOptions.DisableFormatting);
    private static string BuildXml(IEnumerable<WfActivityDetail> rows) => new XElement("Details",rows.OrderBy(x=>x.SortOrder).Select(x=>new XElement("Control",new XElement("ControlDataId",x.ControlDataId),new XElement("ControlLabel",x.ControlLabel),new XElement("ControlLabelAR",x.ControlLabelAR),new XElement("ControlValue",x.ControlValue),new XElement("ControlId",x.ControlId),new XElement("UsedAsCriteria",x.UsedAsCriteria),new XElement("ControlOrder",x.SortOrder),new XElement("RelatedObjectId",0),new XElement("ControlValueAR",x.ControlValueAR),new XElement("ControlValueEN",x.ControlValueEN)))).ToString(SaveOptions.DisableFormatting);

    private static async System.Threading.Tasks.Task AddMissingAsync<TEntity>(ApplicationDbContext db,DbSet<TEntity> set,IEnumerable<TEntity> source,CancellationToken ct) where TEntity:class,IBaseEntity
    {
        var rows=source.ToList();
        var existing=(await set.IgnoreQueryFilters().AsNoTracking().ToListAsync(ct)).Select(x=>x.RecId).ToHashSet();
        var missing=rows.Where(x=>!existing.Contains(x.RecId)).ToList();
        if(missing.Count==0)return;
        var entityType=db.Model.FindEntityType(typeof(TEntity))??throw new InvalidOperationException($"Missing EF metadata for {typeof(TEntity).Name}.");
        var tableName=entityType.GetTableName()??throw new InvalidOperationException($"Missing table mapping for {typeof(TEntity).Name}.");
        var schema=entityType.GetSchema();
        var table=string.IsNullOrWhiteSpace(schema)?SqlIdentifier(tableName):$"{SqlIdentifier(schema)}.{SqlIdentifier(tableName)}";
        await set.AddRangeAsync(missing,ct);
        await SaveIdentityAsync(db,table,ct);
    }

    private static string SqlIdentifier(string value)=>$"[{value.Replace("]","]]",StringComparison.Ordinal)}]";

    private static async System.Threading.Tasks.Task SaveIdentityAsync(ApplicationDbContext db, string table, CancellationToken ct)
    {
        await db.Database.OpenConnectionAsync(ct);
        try { await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT "+table+" ON",ct); await db.SaveChangesAsync(ct); await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT "+table+" OFF",ct); }
        finally { await db.Database.CloseConnectionAsync(); }
    }
}
