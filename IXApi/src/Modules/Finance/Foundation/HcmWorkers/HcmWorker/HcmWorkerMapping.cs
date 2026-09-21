using Mapster;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;

public sealed class HcmWorkerMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<HcmWorker, HcmWorkerDto>()
            .Map(destination => destination.Name, source => source.Party.Name)
            .Map(destination => destination.NameAlias, source => source.Party.NameAlias)
            .Map(destination => destination.OccupationName, source => source.Occupation.Name)
            .Map(destination => destination.GenderName, source => source.Gender.Name)
            .Map(destination => destination.NationalityName, source => source.Nationality.Name);

        config.NewConfig<HcmWorkerDto, HcmWorker>()
            .Ignore(destination => destination.Person)
            .Ignore(destination => destination.Party)
            .Ignore(destination => destination.Occupation)
            .Ignore(destination => destination.Gender)
            .Ignore(destination => destination.Nationality)
            .Ignore(destination => destination.User)
            .Ignore(destination => destination.WorkerOrganizationAssignments);
    }
}
