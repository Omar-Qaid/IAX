using System.Reflection;
using IAX.IXApi.Modules.Identity.Authentication;
using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class ArchitectureTests
{
    private static readonly Assembly ApplicationAssembly = typeof(AuthController).Assembly;

    [Fact]
    public void Production_types_do_not_use_the_legacy_global_namespace()
    {
        var legacyTypes = ApplicationAssembly.GetTypes()
            .Where(type => type.Namespace?.StartsWith("HCMAPIs.Domains.Global", StringComparison.Ordinal) == true)
            .Select(type => type.FullName)
            .OrderBy(name => name)
            .ToArray();

        Assert.Empty(legacyTypes);
    }

    [Theory]
    [InlineData("IAX.IXApi.Modules.Identity.IdentityModule")]
    [InlineData("IAX.IXApi.Modules.Organization.OrganizationModule")]
    [InlineData("IAX.IXApi.Modules.Workflow.WorkflowModule")]
    [InlineData("IAX.IXApi.Modules.ERP.ErpModule")]
    [InlineData("IAX.IXApi.Modules.Communication.CommunicationModule")]
    [InlineData("IAX.IXApi.Modules.Administration.AdministrationModule")]
    public void Every_module_has_an_explicit_composition_entry_point(string typeName)
    {
        Assert.NotNull(ApplicationAssembly.GetType(typeName));
    }

    [Fact]
    public void Entity_framework_model_can_be_constructed_after_reorganization()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=IAX_Architecture_Test;Trusted_Connection=True")
            .Options;

        using var context = new ApplicationDbContext(options);

        Assert.NotEmpty(context.Model.GetEntityTypes());
    }
}
