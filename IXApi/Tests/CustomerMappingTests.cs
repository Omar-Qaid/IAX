using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Customer;
using IAX.IXApi.Modules.Finance.Entities;
using Mapster;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class CustomerMappingTests
{
    [Fact]
    public void Quick_mapping_trims_values_and_preserves_omitted_update_fields()
    {
        var config = new TypeAdapterConfig();
        new CustomerQuickMapping().Register(config);
        var input = new CustomerQuickCreateDto
        {
            Name = "Customer", CustGroupId = " WHOLESALE ", CurrencyCode = " SAR ",
            PaymTermId = " NET30 ", InvoiceAccount = null, Memo = " note "
        };
        var created = input.Adapt<CustTable>(config);
        Assert.Equal("WHOLESALE", created.CustGroupId);
        Assert.Equal("NET30", created.PaymTermId);
        Assert.Equal(string.Empty, created.InvoiceAccount);
        Assert.Equal("note", created.Memo);

        var existing = new CustTable { AccountNum = "C-001", InvoiceAccount = "BILL-1", InventSiteId = "S1" };
        new CustomerQuickUpdateSource(input, existing).Adapt(existing, config);
        Assert.Equal("C-001", existing.AccountNum);
        Assert.Equal("BILL-1", existing.InvoiceAccount);
        Assert.Equal("S1", existing.InventSiteId);
        Assert.Equal(" note ", existing.Memo);
    }

    [Fact]
    public void List_mapping_preserves_customer_fields_and_party_name()
    {
        var config = new TypeAdapterConfig();
        new CustomerMapping().Register(config);
        var customer = new CustTable
        {
            RecId = 42, AccountNum = "C-001", Party = 7, CustGroupId = "WHOLESALE",
            CurrencyCode = "SAR", PaymTermId = "NET30", IsActive = false
        };
        var party = new DirPartyTable { RecId = 7, Name = "Customer", NameAlias = "عميل" };

        var result = new CustomerMappingSource(customer, party).Adapt<CustomerListDto>(config);

        Assert.Equal(42, result.RecId);
        Assert.Equal("C-001", result.AccountNumber);
        Assert.Equal("Customer", result.Name);
        Assert.Equal("عميل", result.NameAr);
        Assert.Equal("WHOLESALE", result.CustomerGroupId);
        Assert.Equal("NET30", result.PaymTermId);
        Assert.Equal("inactive", result.Status);
    }
}
