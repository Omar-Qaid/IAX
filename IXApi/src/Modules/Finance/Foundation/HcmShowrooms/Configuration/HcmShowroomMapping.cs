using IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments;
using Mapster;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;

public sealed class HcmShowroomMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<HcmWorkerShowroomAssignment, HcmShowroomWorkerAssignmentDto>()
            .MapWith(assignment => new HcmShowroomWorkerAssignmentDto(
                assignment.RecId,
                assignment.HcmWorkerId,
                assignment.HcmWorker.PersonnelNumber,
                assignment.HcmWorker.Party.Name,
                assignment.HcmWorker.Party.NameAlias,
                assignment.ValidFrom,
                assignment.ValidTo,
                assignment.IsPrimary,
                assignment.IsActive));
        config.NewConfig<SaveHcmShowroomWorkerAssignmentRequest, HcmWorkerShowroomAssignment>()
            .IgnoreNonMapped(true)
            .Map(d => d.HcmWorkerId, s => s.HcmWorkerId)
            .Map(d => d.ValidFrom, s => s.ValidFrom)
            .Map(d => d.ValidTo, s => s.ValidTo)
            .Map(d => d.IsActive, s => s.IsActive)
            .Map(d => d.IsPrimary, s => true);

        config.NewConfig<HcmShowroom, HcmShowroomDto>()
            .Map(destination => destination.Name, source => source.PartyTable.Name)
            .Map(destination => destination.NameAlias, source => source.PartyTable.NameAlias);

        config.NewConfig<HcmShowroomDto, HcmShowroom>()
            .Ignore(destination => destination.Party)
            .Ignore(destination => destination.PartyTable);
    }
}
