using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Tax;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class TaxModuleArchitectureTests
{
    [Fact]
    public void Tax_owns_its_feature_layout_and_preserves_existing_entity_identity()
    {
        var finance = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../src/Modules/Finance"));
        var tax = Path.Combine(finance, "Tax");
        Assert.True(Directory.Exists(tax));
        Assert.False(Directory.Exists(Path.Combine(finance, "Foundation", "Tax")));
        foreach (var folder in new[] { "Configuration", "Controllers", "Dtos", "Entities", "Interfaces", "Services", "Validation" })
            Assert.True(Directory.Exists(Path.Combine(tax, folder)), folder);
        Assert.Equal("IAX.IXApi.Modules.Finance.Tax", typeof(TaxTableService).Namespace);
        Assert.Equal("IAX.IXApi.Modules.Finance.Tax", typeof(ITaxTableService).Namespace);
        Assert.Equal("IAX.IXApi.Modules.Finance.Entities", typeof(TaxTable).Namespace);
        Assert.DoesNotContain(typeof(TaxTableService).Assembly.GetTypes(), type =>
            type.Namespace?.StartsWith("IAX.IXApi.Modules.Finance.Foundation.Tax", StringComparison.Ordinal) == true);
    }
}
