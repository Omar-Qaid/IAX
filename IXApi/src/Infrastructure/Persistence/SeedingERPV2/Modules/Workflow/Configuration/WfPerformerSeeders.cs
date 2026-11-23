using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Workflow.Performers;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Workflow;

/// <summary>How an activity decides who performs it. A performer stores the identifier of its type.</summary>
public sealed class WfPerformerTypeSeeder : IDataSeeder
{
    public string Id => "ERPV2.WfPerformerTypeSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CompanyInfoSeeder"];

    public Task SeedAsync(SeedContext c, CancellationToken ct) => WorkflowReference.EnsureAsync(c,
    [
        new WfPerformerType { RecId = 1, Code = "ORGANIZATIONAL", Name = "OrganizationalPerformer", NameAlias = "منفذ تنظيمي", SortOrder = 1, IsActive = true },
        new WfPerformerType { RecId = 2, Code = "REQUEST_CONTROL", Name = "RequestControlPerformer", NameAlias = "منفذ من حقل نموذج الطلب", SortOrder = 2, IsActive = true },
        new WfPerformerType { RecId = 3, Code = "ACTIVITY_CONTROL", Name = "ActivityControlPerformer", NameAlias = "منفذ من حقل نموذج النشاط", SortOrder = 3, IsActive = true },
        new WfPerformerType { RecId = 4, Code = "USER", Name = "UserPerformer", NameAlias = "منفذ من المستخدمين المحددين", SortOrder = 4, IsActive = true },
        new WfPerformerType { RecId = 5, Code = "QUERY_DATABASE", Name = "QueryDatabasePerformer", NameAlias = "منفذ استعلام قاعدة البيانات", SortOrder = 5, IsActive = true }
    ], ct);
}

/// <summary>
/// The organizational performers an activity can be assigned to: the applicant, and each reporting
/// line above them. An activity stores the performer identifier, so the identifiers are fixed.
/// </summary>
public sealed class WfPerformerSeeder : IDataSeeder
{
    public string Id => "ERPV2.WfPerformerSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.WfPerformerTypeSeeder"];

    public Task SeedAsync(SeedContext c, CancellationToken ct) => WorkflowReference.EnsureAsync(c,
    [
        new WfPerformer { RecId = 12, Code = "PERF12", Name = "مقدم الطلب", PerformerTypeId = 1, IsApplicant = true, IsEmployee = true, IsActive = true },
        new WfPerformer { RecId = 13, Code = "PERF13", Name = "المدير الاول لمقدم الطلب", PerformerTypeId = 1, IsManager1 = true, IsActive = true },
        new WfPerformer { RecId = 14, Code = "PERF14", Name = "المدير الثاني لمقدم الطلب", PerformerTypeId = 1, IsManager2 = true, IsActive = true },
        new WfPerformer { RecId = 15, Code = "PERF15", Name = "المدير الثالث لمقدم الطلب", PerformerTypeId = 1, IsManager3 = true, IsActive = true },
        new WfPerformer { RecId = 16, Code = "PERF16", Name = "المدير الرابع لمقدم الطلب", PerformerTypeId = 1, IsManager4 = true, IsActive = true }
    ], ct);
}
