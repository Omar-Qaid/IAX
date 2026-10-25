using Mapster;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;

public sealed class HcmWorkerMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<HcmWorker, HcmWorkerDto>()
            .Map(destination => destination.Name, source => source.Party.Name)
            .Map(destination => destination.NameAlias, source => source.Party.NameAlias)
            .Map(destination => destination.OccupationId, source => source.WorkerOrganizationAssignmentsV1
                .Where(assignment => assignment.IsPrimary)
                .OrderByDescending(assignment => assignment.ValidFrom)
                .Select(assignment => assignment.OccupationId ?? 0)
                .FirstOrDefault())
            .Map(destination => destination.OccupationName, source => source.WorkerOrganizationAssignmentsV1
                .Where(assignment => assignment.IsPrimary)
                .OrderByDescending(assignment => assignment.ValidFrom)
                .Select(assignment => assignment.Occupation != null ? assignment.Occupation.Name : null)
                .FirstOrDefault())
            .Map(destination => destination.ManagerWorkerId, source => source.WorkerOrganizationAssignmentsV1
                .Where(assignment => assignment.IsPrimary)
                .OrderByDescending(assignment => assignment.ValidFrom)
                .Select(assignment => (long?)assignment.HcmManagerWorkerId)
                .FirstOrDefault())
            .Map(destination => destination.DepartmentId, source => source.WorkerOrganizationAssignmentsV1
                .Where(assignment => assignment.IsPrimary)
                .OrderByDescending(assignment => assignment.ValidFrom)
                .Select(assignment => assignment.DepartmentId)
                .FirstOrDefault())
            .Map(destination => destination.ShowroomId, source => source.WorkerShowroomAssignments
                .Where(assignment => assignment.IsPrimary)
                .OrderByDescending(assignment => assignment.ValidFrom)
                .Select(assignment => (long?)assignment.HcmShowroomId)
                .FirstOrDefault())
            .Map(destination => destination.GenderName, source => source.Gender.Name)
            .Map(destination => destination.NationalityName, source => source.Nationality.Name);

        config.NewConfig<HcmWorkerDto, HcmWorker>()
            .Ignore(destination => destination.Person)
            .Ignore(destination => destination.Party)
            .Ignore(destination => destination.Gender)
            .Ignore(destination => destination.Nationality)
            .Ignore(destination => destination.User)
            .Ignore(destination => destination.WorkerOrganizationAssignments);
    }
}
