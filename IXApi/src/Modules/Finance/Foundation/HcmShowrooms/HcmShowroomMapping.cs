using Mapster;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;

public sealed class HcmShowroomMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<HcmShowroom, HcmShowroomDto>()
            .Map(destination => destination.Name, source => source.PartyTable.Name)
            .Map(destination => destination.NameAlias, source => source.PartyTable.NameAlias);

        config.NewConfig<HcmShowroomDto, HcmShowroom>()
            .Ignore(destination => destination.Party)
            .Ignore(destination => destination.PartyTable);
    }
}
