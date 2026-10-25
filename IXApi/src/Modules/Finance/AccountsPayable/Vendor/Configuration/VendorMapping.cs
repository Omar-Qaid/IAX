using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using Mapster;

namespace IAX.IXApi.Modules.Finance.AccountsPayable.Configuration;

public sealed class VendorMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<VendTable, VendorDto>()
            .Map(destination => destination.Blocked, source => source.Blocked != CustVendBlocked.No);

        config.NewConfig<VendorDto, VendTable>()
            .Map(destination => destination.Blocked,
                source => source.Blocked ? CustVendBlocked.All : CustVendBlocked.No)
            .Ignore(destination => destination.RecId);

        config.NewConfig<VendGroup, VendorGroupDto>()
            .Map(destination => destination.VendGroup, source => source.VendGroupCode);

        config.NewConfig<VendorGroupDto, VendGroup>()
            .Map(destination => destination.VendGroupCode, source => source.VendGroup)
            .Ignore(destination => destination.RecId);
    }
}
