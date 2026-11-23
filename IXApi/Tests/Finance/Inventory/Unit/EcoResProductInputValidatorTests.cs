using IAX.IXApi.Modules.Finance.Inventory;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class EcoResProductInputValidatorTests
{
    [Theory]
    [InlineData("Item", true)]
    [InlineData("Service", false)]
    [InlineData("BOM", false)]
    [InlineData("ProductMaster", false)]
    [InlineData("unknown", false)]
    public void Product_type_is_limited_to_basic_distinct_item(string productType, bool expectedValid)
    {
        var input = new EcoResProductInputDto
        {
            ProductNumber = "TEST-001",
            Name = "Test product",
            NameAlias = "منتج اختبار",
            ProductType = productType
        };

        var result = new EcoResProductInputValidator().Validate(input);

        Assert.Equal(expectedValid, result.IsValid);
    }
}
