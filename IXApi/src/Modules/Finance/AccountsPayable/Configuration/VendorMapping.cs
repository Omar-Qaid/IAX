using IAX.IXApi.Modules.Finance.Entities;
using Mapster;

namespace IAX.IXApi.Modules.Finance.AccountsPayable.Configuration;

public sealed class VendorMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<VendGroup, VendorGroupDto>()
            .Map(destination => destination.VendGroup, source => source.VendGroupCode);

        config.NewConfig<VendorGroupDto, VendGroup>()
            .Map(destination => destination.VendGroupCode, source => source.VendGroup)
            .Ignore(destination => destination.RecId);
    }
}
