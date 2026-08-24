using System.Xml.Linq;
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
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Steps;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>Legacy process 662 plus the complete workflow-only execution for request 94037.</summary>
public sealed class WorkflowRequestTrackingSeeder : ISeeder
{
    private const long ProcessId = 662;
    // The source signatures exceed WfRequestDetail.ControlValue's 255 character limit.
    private const string Signature = "data:image/svg+xml;base64,PHN2Zy8+";

    private sealed record RequestControl(long Id, byte Type, string Code, string Label, string LabelAr, byte Order, bool Mandatory = false, bool Active = true, string? Properties = null);
    private sealed record ActivityControl(long Id, long Activity, byte Type, string Code, string Label, string LabelAr, byte Order, bool Mandatory, string? Properties = null);
    private sealed record Request(long Id, DateTime Date, DateTime Finished, string CreatedBy);
    private sealed record RequestValue(long Id, long Request, byte Type, long Control, string Label, string LabelAr, string Value, bool Criteria, byte Order, string ValueAr = "", string ValueEn = "");
    private sealed record Assignment(long Id, long Request, long Activity, long User, DateTime Assigned, DateTime Finished, long Step, bool AutoPassing, byte Hours, bool Automatically);
    private sealed record Task(long Id, long Assignment, DateTime Finished);
    private sealed record ActivityValue(long Id, long Task, long Assignment, byte Type, long Control, string Label, string LabelAr, string Value, byte Order, string ValueAr, string ValueEn);
    private sealed record Employee(long Id, string Code, string Name, string NameAr, short Department, short Occupation, byte Gender, short Nationality, DateTime Created);

    private static readonly Employee[] Employees =
    [
        new(155350,"F171","Noura Faraaj Dugheim Alsuhli","نوره فرج دغيم السهلي",114,3,2,1,new(2024,8,23,19,43,56,293)),
        new(157077,"muhaseb","محاسب المبيعات اليومية إغلاق صندوق","محاسب المبيعات اليومية إغلاق صندوق",118,44,1,1,new(2025,3,4,22,50,11,872)),
        new(157424,"D141","Wataniya Centre - Hafar Al Batin","الوطنية سنتر حفر الباطن",134,499,1,1,new(2025,7,26,18,11,50,606)),
    ];

    private static readonly RequestControl[] RequestControls =
    [
        new(21201,1,"TOTAL_SALES","Total sales","إجمالي المبيعات:-",0), new(21202,1,"CASH","cash","نقدي",1),
        new(21169,9,"SPLIT_PAYMENTS","Split payments","تقسيم المدفوعات",2,Active:false,Properties:"<Root><row><col1>الاسم</col1><col2>المبلغ</col2></row></Root>"),
        new(21204,1,"MADA","Mada","مدى",2), new(21205,1,"VISA","Visa","فيزا",3), new(21206,1,"MASTERCARD","MasterCard","ماستر كارد",4),
        new(21207,1,"AMERICAN_EXPRESS","American Express","امريكان اكسبرس",5), new(21208,1,"GULF_NETWORK","Gulf Network","شبكة خليجية",6),
        new(21209,1,"TABBY","Tabby","تابي",7), new(21210,1,"TAMARA","Tamara","تمارا",8),
        new(21162,4,"CLOSING_DATE","Today's closing date","تاريخ اقفال اليوم",9,Mandatory:true),
        new(21166,12,"CLOSING_EMPLOYEE","Locksmith Name Today","اسم موظف الاقفال اليوم",10,Mandatory:true),
        new(21167,20,"CLOSING_SIGNATURE","signature","التوقيع",11), new(21172,10,"ATTACHMENT_HEADING","The following must be attached:","يجب ارفاق الاتي:-",12,Properties:"#fb1818"),
        new(21173,10,"BUDGET_IMAGE_LABEL","Budget image","صور واضحة للموازنة",13), new(21174,10,"NETWORK_RECEIPTS_LABEL","Image of network receipts","صورة ايصالات الشبكة",14),
        new(21168,3,"REQUEST_NOTE","note","ملاحظة",15),
    ];

    private static readonly ActivityControl[] ActivityControls =
    [
        new(38712,6461,6,"LOCKS_COMPATIBLE","Are the locks compatible?","هل الاقفال مطابق",1,false,"<Data><Item><ar>نعم</ar><en>YES</en><value>YES</value></Item><Item><ar>لا</ar><en>NO</en><value>NO</value></Item></Data>"),
        new(38713,6461,3,"REVIEW_NOTE","note","ملاحظة",2,false),
        new(38687,6452,6,"SEEN","Seen","تم الاطلاع",1,true,"<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item></Data>"),
    ];

    private static readonly Request[] Requests =
    [
        new(94037,new(2025,12,15,0,3,58,290),new(2025,12,15,10,19,10,840),"157424"),
    ];

    private static readonly RequestValue[] Values =
    [
        new(685037,94037,1,21201,"Total sales","إجمالي المبيعات:-","5239",false,0), new(685038,94037,1,21202,"cash","نقدي","2206",false,1),
        new(685039,94037,1,21204,"Mada","مدى","1753",false,2), new(685040,94037,1,21205,"Visa","فيزا","762",false,3),
        new(685041,94037,1,21206,"MasterCard","ماستر كارد","69",false,4), new(685042,94037,1,21207,"American Express","امريكان اكسبرس","0",false,5),
        new(685043,94037,1,21208,"Gulf Network","شبكة خليجية","254",false,6), new(685044,94037,1,21209,"Tabby","تابي","195",false,7), new(685045,94037,1,21210,"Tamara","تمارا","0",false,8),
        new(685046,94037,4,21162,"Today's closing date","تاريخ اقفال اليوم","2025-12-14",true,9), new(685047,94037,12,21166,"Locksmith Name Today","اسم موظف الاقفال اليوم","157430",true,10,"فهد مشعل الشمري","فهد مشعل الشمري"),
        new(685048,94037,20,21167,"signature","التوقيع",Signature,false,11), new(685049,94037,10,21172,"The following must be attached:","يجب ارفاق الاتي:-","",false,12),
        new(685050,94037,10,21173,"Budget image","صور واضحة للموازنة","",false,13), new(685051,94037,10,21174,"Image of network receipts","صورة ايصالات الشبكة","",false,14), new(685052,94037,3,21168,"note","ملاحظة","",false,15),
    ];

    private static readonly Assignment[] Assignments =
    [
        new(321331,94037,6461,155350,new(2025,12,15,0,3,58,350),new(2025,12,15,9,18,18,560),16103,false,0,false),
        new(321463,94037,6452,157077,new(2025,12,15,9,18,18,623),new(2025,12,15,10,19,10,797),16098,true,1,true),
    ];

    private static readonly Task[] Tasks =
    [
        new(313179,321331,new(2025,12,15,9,18,18,557)), new(313322,321463,new(2025,12,15,10,19,10,797)),
    ];

    private static readonly ActivityValue[] ActivityValues =
    [
        new(383793,313179,321331,6,38712,"Are the locks compatible?","هل الاقفال مطابق","YES",1,"نعم","YES"), new(383794,313179,321331,3,38713,"note","ملاحظة","",2,"",""),
        new(384135,313322,321463,10,0,"The transaction was processed.","تم تمرير المعاملة","تم تمرير المعاملة آلياً",0,"تم تمرير المعاملة آلياً","The transaction was processed automatically."),
    ];

    public async System.Threading.Tasks.Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
    {
        _ = roles;
        var createdBy = (await users.FindByNameAsync("sys"))?.Id ?? "sys";
        await SeedPrerequisitesAsync(db, createdBy, ct);
        await SeedDefinitionAsync(db, createdBy, ct);
        await SeedExecutionAsync(db, createdBy, ct);
    }

    private static async System.Threading.Tasks.Task SeedPrerequisitesAsync(ApplicationDbContext db, string by, CancellationToken ct)
    {
        // Only the organization records referenced by request 94037 and its assignments.
        await AddMissingAsync(db, db.Departments, new[]
        {
            new OrgDepartment {RecId=114,Code="DEP114",Name="Sales Department",Description="إدارة المبيعات",IsActive=true,CreatedBy=by,OwnerAccountId=by},
            new OrgDepartment {RecId=118,Code="DEP118",Name="Financial Department",Description="الإدارة المالية",IsActive=true,CreatedBy=by,OwnerAccountId=by},
            new OrgDepartment {RecId=134,Code="DEP134",Name="Branch management",Description="إدارة الفروع",IsActive=true,CreatedBy=by,OwnerAccountId=by},
        },ct);
        await AddMissingAsync(db, db.Occupations, new[]
        {
            new OrgOccupation {RecId=3,Code="OCC3",Name="Area Supervisor",Description="مشرف منطقة",IsActive=true,CreatedBy=by,OwnerAccountId=by},
            new OrgOccupation {RecId=44,Code="OCC44",Name="Financial Controller",Description="المراقب المالي",IsActive=true,CreatedBy=by,OwnerAccountId=by},
            new OrgOccupation {RecId=499,Code="OCC499",Name="Showroom",Description="معرض",IsActive=true,CreatedBy=by,OwnerAccountId=by},
        },ct);
        await AddMissingAsync(db, db.Genders, new[]
        {
            new OrgGender {RecId=1,Code="GEN1",Name="ذكر",Description="ذكر",IsActive=true,CreatedBy=by,OwnerAccountId=by},
            new OrgGender {RecId=2,Code="GEN2",Name="أنثى",Description="أنثى",IsActive=true,CreatedBy=by,OwnerAccountId=by},
        },ct);
        await AddMissingAsync(db, db.Nationalities, new[]
        {
            new OrgNationality {RecId=1,Code="NAT1",Name="سعودي",Description="سعودي",IsActive=true,CreatedBy=by,OwnerAccountId=by},
        },ct);
        await SeedEmployeesAsync(db,by,ct);

        // Only workflow masters referenced by process 662.
        await AddMissingAsync(db,db.WfPriorities,new[]{new WfPriority {RecId=1,Code="LOW",Name="Low",Description="Low Priority",SortOrder=1,IsActive=true,CreatedBy=by,OwnerAccountId=by}},ct);
        await AddMissingAsync(db,db.WfProcessTypes,new[]{new WfProcessType {RecId=1,Code="STD",Name="Standard",IsActive=true,CreatedBy=by,OwnerAccountId=by}},ct);
        await AddMissingAsync(db,db.WfPerformerTypes,new[]{new WfPerformerType {RecId=1,Code="RELATIONAL",Name="Relational",SortOrder=1,IsActive=true,CreatedBy=by,OwnerAccountId=by}},ct);
        await AddMissingAsync(db,db.WfActivityTypes,new[]{new WfActivityType {RecId=2,Code="NORMAL",Name="Normal",Description="Normal Activity",SortOrder=2,IsActive=true,CreatedBy=by,OwnerAccountId=by}},ct);
        await AddMissingAsync(db,db.WfCategories,new[]{new WfCategory {RecId=14,Code="CAT14",Name="Branch management transactions",Description="معاملات إدراة المعارض",SortOrder=14,IsActive=true,CreatedBy=by,OwnerAccountId=by}},ct);
        await AddMissingAsync(db,db.WfControls,new[]
        {
            new WfControl {RecId=1,Code="number",Name="Digits",Description="مربع رقمي",ControlType="TextBox",SortOrder=1,IsActive=true,CreatedBy=by,OwnerAccountId=by},
            new WfControl {RecId=3,Code="textarea",Name="Long text",Description="نص طويل",ControlType="TextBox",SortOrder=3,IsActive=true,CreatedBy=by,OwnerAccountId=by},
            new WfControl {RecId=4,Code="date",Name="Date",Description="تاريخ",ControlType="Calendar",SortOrder=4,IsActive=true,CreatedBy=by,OwnerAccountId=by},
            new WfControl {RecId=6,Code="select",Name="Drop Down List (Fill Manually)",Description="قائمة منسدلة (تعبأ يدويا)",ControlType="DropDownList",SortOrder=6,IsActive=true,CreatedBy=by,OwnerAccountId=by},
            new WfControl {RecId=10,Code="label",Name="Label",Description="نص للقراءة فقط",ControlType="Label",SortOrder=10,IsActive=true,CreatedBy=by,OwnerAccountId=by},
            new WfControl {RecId=12,Code="search",Name="EmployeeSearch",Description="بحث في الموظفين",ControlType="ComboBox",SortOrder=12,IsActive=true,CreatedBy=by,OwnerAccountId=by},
            new WfControl {RecId=20,Code="Signature",Name="Signature",Description="توقيع",ControlType="Signature",SortOrder=20,IsActive=true,CreatedBy=by,OwnerAccountId=by},
        },ct);
    }

    private static async System.Threading.Tasks.Task SeedEmployeesAsync(ApplicationDbContext db,string by,CancellationToken ct)
    {
        var parties=(await db.DirPartyTables.IgnoreQueryFilters().Where(x=>x.HcmWorker.HasValue).ToListAsync(ct)).GroupBy(x=>x.HcmWorker!.Value).ToDictionary(x=>x.Key,x=>x.First());
        foreach(var row in Employees.Where(x=>!parties.ContainsKey(x.Id)))
        {
            var party=new DirPartyTable {PartyNumber=row.Code,Name=row.Name,NameAlias=row.Code,RFullName=row.NameAr,LanguageId="ar-sa",AddressBookNames="",HcmWorker=row.Id,IsActive=NoYes.Yes,CreatedAt=row.Created,CreatedBy=by,OwnerAccountId=by};
            db.DirPartyTables.Add(party);
            parties[row.Id]=party;
        }
        await db.SaveChangesAsync(ct);

        var existing=(await db.HcmWorkers.IgnoreQueryFilters().AsNoTracking().Select(x=>x.RecId).ToListAsync(ct)).ToHashSet();
        var missing=Employees.Where(x=>!existing.Contains(x.Id)).Select(x=>new HcmWorker {RecId=x.Id,PersonnelNumber=x.Code,Person=parties[x.Id].RecId,DepartmentId=x.Department,OccupationId=x.Occupation,GenderId=x.Gender,NationalityId=x.Nationality,IsActive=true,CreatedAt=x.Created,CreatedBy=by,OwnerAccountId=by}).ToList();
        if(missing.Count==0)return;
        await db.HcmWorkers.AddRangeAsync(missing,ct);
        await SaveIdentityAsync(db,"[HcmWorker]",ct);
    }

    private static async System.Threading.Tasks.Task SeedDefinitionAsync(ApplicationDbContext db, string by, CancellationToken ct)
    {
        foreach (var p in new[]{new WfPerformer {RecId=13,Code="LEGACY_13",Name="المدير الاول للمقدم الطلب",PerformerTypeId=1,IsManager1=true,IsActive=true,CreatedBy=by,OwnerAccountId=by},new WfPerformer {RecId=142,Code="LEGACY_142",Name="محاسب المبيعات اليومية إغلاق",PerformerTypeId=1,IsActive=true,CreatedBy=by,OwnerAccountId=by}})
            if (!await db.WfPerformers.IgnoreQueryFilters().AnyAsync(x=>x.RecId==p.RecId,ct)) { db.WfPerformers.Add(p); await SaveIdentityAsync(db,"WfPerformers",ct); }
        if (!await db.WfProcesses.IgnoreQueryFilters().AnyAsync(x=>x.RecId==ProcessId,ct)) { db.WfProcesses.Add(new WfProcess {RecId=ProcessId,Code="DAILY_FUND_CLOSING",Name="Daily closing of the fund",Description="اقفال اليومي للصندوق",CategoryId=14,PriorityId=1,ProcessTypeId=1,CanRepeat=true,SortOrder=1,IsActive=true,CreatedBy=by,OwnerAccountId=by}); await SaveIdentityAsync(db,"WfProcesses",ct); }
        foreach (var s in new[]{new WfStep {RecId=16103,ProcessId=ProcessId,Code="FIRST_MANAGER",Name="First manager",Description="المدير الاول",SortOrder=1,IsActive=true,CreatedBy=by,OwnerAccountId=by},new WfStep {RecId=16098,ProcessId=ProcessId,Code="FINANCIAL_ACCOUNTANT",Name="محاسب المبيعات اليومية إغلاق صندوق",Description="محاسب المالية",SortOrder=2,IsActive=true,CreatedBy=by,OwnerAccountId=by}})
            if (!await db.WfSteps.IgnoreQueryFilters().AnyAsync(x=>x.RecId==s.RecId,ct)) { db.WfSteps.Add(s); await SaveIdentityAsync(db,"WfSteps",ct); }
        // Legacy type 0 is the normal activity type; this schema uses normal type 2.
        foreach (var a in new[]{new WfActivity {RecId=6461,ActivityTypeId=2,StepId=16103,PerformerId=13,Code="FIRST_MANAGER",Name="First manager",Description="المدير الاول",AlertingBySystem=true,AlertingByEmail=true,ShowPreviousDocs=true,IsActive=true,CreatedBy=by,OwnerAccountId=by},new WfActivity {RecId=6452,ActivityTypeId=2,StepId=16098,PerformerId=142,Code="FINANCIAL_ACCOUNTANT",Name="Financial Accountant",Description="محاسب المالية",ShowPreviousSteps=true,ShowPreviousDocs=true,AutoPassEnabled=true,AutoPassingHrs=1,IsActive=true,CreatedBy=by,OwnerAccountId=by}})
            if (!await db.WfActivities.IgnoreQueryFilters().AnyAsync(x=>x.RecId==a.RecId,ct)) { db.WfActivities.Add(a); await SaveIdentityAsync(db,"WfActivities",ct); }
        foreach (var c in RequestControls)
            if (!await db.WfRequestControls.IgnoreQueryFilters().AnyAsync(x=>x.RecId==c.Id,ct)) { db.WfRequestControls.Add(new WfRequestControl {RecId=c.Id,ProcessId=ProcessId,ControlId=c.Type,Code=c.Code,Name=c.Label,Description=c.LabelAr,SortOrder=c.Order,ValidationRules=c.Mandatory?"<Validation><Required>true</Required></Validation>":null,ExtendedProperties=c.Properties,IsActive=c.Active,CreatedBy=by,OwnerAccountId=by}); await SaveIdentityAsync(db,"WfRequestControls",ct); }
        foreach (var c in ActivityControls)
            if (!await db.WfActivityControls.IgnoreQueryFilters().AnyAsync(x=>x.RecId==c.Id,ct)) { db.WfActivityControls.Add(new WfActivityControl {RecId=c.Id,ActivityId=c.Activity,ProcessId=ProcessId,ControlId=c.Type,Code=c.Code,Name=c.Label,Description=c.LabelAr,SortOrder=c.Order,ValidationRules=c.Mandatory?"<Validation><Required>true</Required></Validation>":null,ExtendedProperties=c.Properties,IsActive=true,CreatedBy=by,OwnerAccountId=by}); await SaveIdentityAsync(db,"WfActivityControls",ct); }
        if (!await db.WfActivityControlsOptions.IgnoreQueryFilters().AnyAsync(x=>x.ActivityControlId==38712,ct)) await db.WfActivityControlsOptions.AddRangeAsync(new WfActivityControlsOption {ActivityControlId=38712,Value="YES",Name="نعم",SortOrder=1,IsActive=true,CreatedBy=by,OwnerAccountId=by},new WfActivityControlsOption {ActivityControlId=38712,Value="NO",Name="لا",SortOrder=2,IsActive=true,CreatedBy=by,OwnerAccountId=by});
        if (!await db.WfActivityControlsOptions.IgnoreQueryFilters().AnyAsync(x=>x.ActivityControlId==38687,ct)) db.WfActivityControlsOptions.Add(new WfActivityControlsOption {ActivityControlId=38687,Value="نعم",Name="نعم",SortOrder=1,IsActive=true,CreatedBy=by,OwnerAccountId=by});
        foreach (var c in RequestControls.Where(x=>x.Mandatory))
            if (!await db.WfRequestControlsValidations.IgnoreQueryFilters().AnyAsync(x=>x.RequestControlId==c.Id && x.ValidationType=="Required",ct)) db.WfRequestControlsValidations.Add(new WfRequestControlsValidation {RequestControlId=c.Id,ValidationType="Required",ErrorMessage=$"{c.LabelAr} مطلوب",Severity="Error",SortOrder=1,IsActive=true,CreatedBy=by,OwnerAccountId=by});
        if (!await db.WfActivityControlsValidations.IgnoreQueryFilters().AnyAsync(x=>x.ActivityControlId==38687 && x.ValidationType=="Required",ct)) db.WfActivityControlsValidations.Add(new WfActivityControlsValidation {ActivityControlId=38687,Code="SEEN_REQUIRED",Name="Seen confirmation required",ValidationType="Required",ErrorMessage="يجب تأكيد الاطلاع",Severity="Error",SortOrder=1,IsActive=true,CreatedBy=by,OwnerAccountId=by});
        await db.SaveChangesAsync(ct);
    }

    private static async System.Threading.Tasks.Task SeedExecutionAsync(ApplicationDbContext db, string by, CancellationToken ct)
    {
        var details=Values.Where(v=>v.Request==94037).Select(v=>new WfRequestDetail {RecId=v.Id,ProcessId=ProcessId,RequestId=v.Request,ControlId=v.Type,ControlDataId=v.Control,ControlLabel=v.Label,ControlLabelAR=v.LabelAr,ControlValue=v.Value,ControlValueAR=v.ValueAr,ControlValueEN=v.ValueEn,UsedAsCriteria=v.Criteria,SortOrder=v.Order,CreatedBy=by,OwnerAccountId=by}).ToList();
        foreach (var r in Requests.Where(x=>x.Id==94037))
            if (!await db.WfRequests.IgnoreQueryFilters().AnyAsync(x=>x.RecId==r.Id,ct)) { db.WfRequests.Add(new WfRequest {RecId=r.Id,Code=r.Id.ToString(),Name=$"Daily fund closing - {r.Date:yyyy-MM-dd}",ProcessId=ProcessId,EmployeeId=157424,RequestDate=r.Date,RequestDetails=BuildXml(details.Where(x=>x.RequestId==r.Id)),IsFinished=true,FinishedDate=r.Finished,Progress=100,IsActive=true,CreatedAt=r.Date,CreatedBy=r.CreatedBy,OwnerAccountId=by}); await SaveIdentityAsync(db,"WfRequests",ct); }
        foreach (var d in details)
            if (!await db.WfRequestDetails.IgnoreQueryFilters().AnyAsync(x=>x.RecId==d.RecId,ct)) { db.WfRequestDetails.Add(d); await SaveIdentityAsync(db,"WfRequestDetails",ct); }
        foreach (var a in Assignments.Where(x=>x.Request==94037))
            if (!await db.WfAssignments.IgnoreQueryFilters().AnyAsync(x=>x.RecId==a.Id,ct)) { db.WfAssignments.Add(new WfAssignment {RecId=a.Id,RequestId=a.Request,ActivityId=a.Activity,UserId=a.User,AssignDate=a.Assigned,IsFinished=true,FinishedDate=a.Finished,AutoPassing=a.AutoPassing,AutoPassingHrs=a.Hours,StepId=a.Step,Automatically=a.Automatically,CreatedBy=by,OwnerAccountId=by}); await SaveIdentityAsync(db,"WfAssignments",ct); }
        var selectedAssignmentIds=Assignments.Where(x=>x.Request==94037).Select(x=>x.Id).ToHashSet();
        var activityDetails=ActivityValues.Where(v=>selectedAssignmentIds.Contains(v.Assignment)).Select(v=>new WfActivityDetail {RecId=v.Id,ProcessId=v.Task,AssignmentID=v.Assignment,ControlId=v.Type,ControlDataId=v.Control,ControlLabel=v.Label,ControlLabelAR=v.LabelAr,ControlValue=v.Value,ControlValueAR=v.ValueAr,ControlValueEN=v.ValueEn,SortOrder=v.Order,CreatedBy=by,OwnerAccountId=by}).ToList();
        foreach (var t in Tasks.Where(x=>selectedAssignmentIds.Contains(x.Assignment)))
            if (!await db.WfProcessData.IgnoreQueryFilters().AnyAsync(x=>x.RecId==t.Id,ct)) { db.WfProcessData.Add(new WfProcessData {RecId=t.Id,AssignmentID=t.Assignment,FinishDate=t.Finished,ActivityDetails=BuildXml(activityDetails.Where(x=>x.ProcessId==t.Id)),CreatedBy=by,OwnerAccountId=by}); await SaveIdentityAsync(db,"WfProcessData",ct); }
        foreach (var d in activityDetails)
            if (!await db.WfActivityDetails.IgnoreQueryFilters().AnyAsync(x=>x.RecId==d.RecId,ct)) { db.WfActivityDetails.Add(d); await SaveIdentityAsync(db,"WfActivityDetails",ct); }
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
