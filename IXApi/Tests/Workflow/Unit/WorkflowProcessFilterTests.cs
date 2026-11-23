using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Steps;
using IAX.IXApi.Modules.Workflow.Variables;
using IAX.IXApi.Modules.Workflow.Transitions;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.Querying;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class WorkflowProcessFilterTests
{
    [Fact]
    public void Builder_filters_translate_to_SQL_without_loading_unrelated_processes()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=IAX_Query_Test;Trusted_Connection=True")
            .Options;
        using var db = new ApplicationDbContext(options);
        Check<WfVariable>(db, "ProcessId");
        Check<WfStep>(db, "ProcessId");
        Check<WfActivity>(db, "Step.ProcessId");
        Check<WfActivityControl>(db, "Activity.Step.ProcessId");
        Check<WfActivityControlsValidation>(db, "ActivityControl.Activity.Step.ProcessId");
        Check<WfActivityControlsOption>(db, "ActivityControl.Activity.Step.ProcessId");
        Check<WfRequestControl>(db, "ProcessId");
        Check<WfRequestControlsValidation>(db, "RequestControl.ProcessId");
        Check<WfRequestControlsOption>(db, "RequestControl.ProcessId");
        Check<WfTransition>(db, "ProcessId");
    }

    private static void Check<T>(ApplicationDbContext db, string field) where T : class
    {
        var filter = new QueryFilterDto
        {
            SortField = "RecId", SortOrder = "asc", PageSize = 100,
            Filters = [new() { Field = field, Operator = "equals", Value = "42" }],
        };
        // ToQueryString translates the real EF model without connecting to a database.
        var sql = db.Set<T>().AsNoTracking().WhereDataGrid(filter).Take(100).ToQueryString();
        Assert.Contains("WHERE", sql);
        Assert.Contains("ProcessId", sql);
        Assert.Contains("42", sql);
        Assert.Contains("ORDER BY", sql);
    }
}
