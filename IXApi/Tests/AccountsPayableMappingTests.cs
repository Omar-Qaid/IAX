using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsPayable.Configuration;
using IAX.IXApi.Modules.Finance.Entities;
using Mapster;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class AccountsPayableMappingTests
{
    [Fact]
    public void Vendor_blocked_boolean_maps_to_purchase_block_without_changing_identity()
    {
        var config = new TypeAdapterConfig();
        new VendorMapping().Register(config);
        var source = new VendTable { RecId = 42, AccountNum = "V-42", Blocked = IAX.IXApi.Modules.Finance.Common.CustVendBlocked.Invoicing };
        Assert.True(source.Adapt<VendorDto>(config).Blocked);

        new VendorDto { RecId = 99, Blocked = true }.Adapt(source, config);
        Assert.Equal(IAX.IXApi.Modules.Finance.Common.CustVendBlocked.All, source.Blocked);
        Assert.Equal(42, source.RecId);
    }

    [Fact]
    public void Vendor_group_code_maps_in_both_directions_without_replacing_identity()
    {
        var config = new TypeAdapterConfig();
        new VendorMapping().Register(config);

        var source = new VendGroup { RecId = 42, VendGroupCode = "TRADE", Name = "Trade" };
        var dto = source.Adapt<VendorGroupDto>(config);
        Assert.Equal("TRADE", dto.VendGroup);

        dto.VendGroup = "LOCAL";
        dto.RecId = 99;
        dto.Adapt(source, config);
        Assert.Equal("LOCAL", source.VendGroupCode);
        Assert.Equal(42, source.RecId);
    }
}
