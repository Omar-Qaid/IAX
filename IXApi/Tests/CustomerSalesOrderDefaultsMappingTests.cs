using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Customer;
using IAX.IXApi.Modules.Finance.Common;
using Mapster;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class CustomerSalesOrderDefaultsMappingTests
{
    [Fact]
    public void Maps_address_and_contact_defaults()
    {
        new CustomerMapping().Register(TypeAdapterConfig.GlobalSettings);
        var address = new CustomerSalesOrderAddressSource(123, "Main street", true)
            .Adapt<CustomerSalesOrderAddressDto>();
        var contact = new CustomerSalesOrderContactSource(ElectronicAddressType.Email, "customer@example.com", false)
            .Adapt<CustomerSalesOrderContactDto>();
        var defaults = new CustomerSalesOrderDefaultsSource("Main street", [address], [contact])
            .Adapt<CustomerSalesOrderDefaultsDto>();

        Assert.Equal("Main street", defaults.Address);
        Assert.Equal("123", defaults.Addresses[0].Id);
        Assert.True(defaults.Addresses[0].Primary);
        Assert.Equal("Email", defaults.Contacts[0].Type);
        Assert.Equal("customer@example.com", defaults.Contacts[0].Number);
        Assert.False(defaults.Contacts[0].Primary);
    }
}
