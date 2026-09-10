using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Scheduling;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IXApi.Tests;

public class WorkflowScheduleFoundationTests
{
    [Fact]
    public void BackgroundScopeSuppliesCompanyAndActorWithoutHttpContext()
    {
        var identity = new BackgroundExecutionIdentity();
        identity.Initialize("scheduler-account", "USMF", "owner-account");
        var accessor = new HttpContextAccessor();
        var company = new CompanyExecutionContext(accessor, identity);
        var user = new CurrentUserService(accessor, company, identity);
        Assert.Equal("USMF", user.GetDataAreaId());
        Assert.Equal("scheduler-account", user.GetCurrentUserId());
        Assert.Equal("owner-account", user.GetOwnerAccountId());
        Assert.Throws<InvalidOperationException>(() => identity.Initialize("another", "DEMF", "another"));
        Assert.Equal("dat", new CompanyExecutionContext(accessor).GetDataAreaId());
    }

    [Fact]
    public void ScheduleHasUniqueCompanyProcessAndConcurrencyToken()
    {
        using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=true;TrustServerCertificate=true").Options);
        var entity = db.Model.FindEntityType(typeof(WFProcessScheduled))!;
        Assert.Equal("WFProcessScheduled", entity.GetTableName());
        Assert.True(entity.FindProperty(nameof(WFProcessScheduled.RowVersion))!.IsConcurrencyToken);
        Assert.Contains(entity.GetIndexes(), index => index.IsUnique
            && index.Properties.Select(x => x.Name).SequenceEqual(new[] { "DataAreaId", "ProcessId" }));
        Assert.False(db.Database.HasPendingModelChanges());
    }
}
