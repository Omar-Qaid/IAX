using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Operators;
using IAX.IXApi.Modules.Workflow.Priorities;
using IAX.IXApi.Modules.Workflow.ProcessTypes;
using IAX.IXApi.Modules.Workflow.Variables;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Workflow;

/// <summary>The priority a request is raised at. A process and a request both store its identifier.</summary>
public sealed class WfPrioritySeeder : IDataSeeder
{
    public string Id => "ERPV2.WfPrioritySeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CompanyInfoSeeder"];

    public Task SeedAsync(SeedContext c, CancellationToken ct) => WorkflowReference.EnsureAsync(c,
    [
        new WfPriority { RecId = 1, Code = "LOW", Name = "Low", Description = "Low Priority", SortOrder = 1, IsActive = true },
        new WfPriority { RecId = 2, Code = "MED", Name = "Medium", Description = "Medium Priority", SortOrder = 2, IsActive = true },
        new WfPriority { RecId = 3, Code = "HIGH", Name = "High", Description = "High Priority", SortOrder = 3, IsActive = true }
    ], ct);
}

/// <summary>How a process is run: straight through, as a review, or as an approval.</summary>
public sealed class WfProcessTypeSeeder : IDataSeeder
{
    public string Id => "ERPV2.WfProcessTypeSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CompanyInfoSeeder"];

    public Task SeedAsync(SeedContext c, CancellationToken ct) => WorkflowReference.EnsureAsync(c,
    [
        new WfProcessType { RecId = 1, Code = "STD", Name = "Standard", IsActive = true },
        new WfProcessType { RecId = 2, Code = "REV", Name = "Review", IsActive = true },
        new WfProcessType { RecId = 3, Code = "APP", Name = "Approval", IsActive = true }
    ], ct);
}

/// <summary>
/// Whether an activity completes the step on its own or only in part. Identifier 2 is the normal
/// activity: a zero key is the sentinel an unset foreign key reads as, so it cannot also be a row.
/// </summary>
public sealed class WfActivityTypeSeeder : IDataSeeder
{
    public string Id => "ERPV2.WfActivityTypeSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CompanyInfoSeeder"];

    public Task SeedAsync(SeedContext c, CancellationToken ct) => WorkflowReference.EnsureAsync(c,
    [
        new WfActivityType { RecId = 1, Code = "PARTIAL", Name = "مرحلة جزئية", Description = "مرحلة جزئية", SortOrder = 1, IsActive = true },
        new WfActivityType { RecId = 2, Code = "NORMAL", Name = "مرحلة عادية", Description = "مرحلة عادية", SortOrder = 2, IsActive = true }
    ], ct);
}

/// <summary>The comparisons a transition condition is evaluated with.</summary>
public sealed class WfOperatorSeeder : IDataSeeder
{
    public string Id => "ERPV2.WfOperatorSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CompanyInfoSeeder"];

    public Task SeedAsync(SeedContext c, CancellationToken ct) => WorkflowReference.EnsureAsync(c,
    [
        new WfOperator { RecId = 1, Code = "GT", Name = ">", Description = ">", SortOrder = 1, IsActive = true },
        new WfOperator { RecId = 2, Code = "LT", Name = "<", Description = "<", SortOrder = 2, IsActive = true },
        new WfOperator { RecId = 3, Code = "GTE", Name = ">=", Description = ">=", SortOrder = 3, IsActive = true },
        new WfOperator { RecId = 4, Code = "LTE", Name = "<=", Description = "<=", SortOrder = 4, IsActive = true },
        new WfOperator { RecId = 5, Code = "EQ", Name = "=", Description = "=", SortOrder = 5, IsActive = true },
        new WfOperator { RecId = 6, Code = "NEQ", Name = "<>", Description = "<>", SortOrder = 6, IsActive = true },
        new WfOperator { RecId = 7, Code = "BETWEEN", Name = "Between", Description = "Between", SortOrder = 7, IsActive = true }
    ], ct);
}

/// <summary>The types a process variable can hold.</summary>
public sealed class WfDataTypeSeeder : IDataSeeder
{
    public string Id => "ERPV2.WfDataTypeSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CompanyInfoSeeder"];

    public Task SeedAsync(SeedContext c, CancellationToken ct) => WorkflowReference.EnsureAsync(c,
    [
        new WfDataType { RecId = 1, Code = "INT", Name = "Integer", Description = "Integer", SortOrder = 1, IsActive = true },
        new WfDataType { RecId = 2, Code = "STR", Name = "String", Description = "String", SortOrder = 2, IsActive = true },
        new WfDataType { RecId = 3, Code = "DT", Name = "Date/Time", Description = "Date/Time", SortOrder = 3, IsActive = true },
        new WfDataType { RecId = 4, Code = "BOOL", Name = "True/False", Description = "True/False", SortOrder = 4, IsActive = true }
    ], ct);
}
