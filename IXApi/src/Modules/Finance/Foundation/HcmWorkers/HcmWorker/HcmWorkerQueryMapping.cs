using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using Mapster;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;

public sealed class HcmWorkerListProjectionDto : HcmWorkerDto { }

public sealed class HcmWorkerQueryMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<HcmWorker, HcmWorkerListProjectionDto>().IgnoreNonMapped(true)
            .Map(d => d.RecId, worker => worker.RecId)
            .Map(d => d.PersonnelNumber, worker => worker.PersonnelNumber)
            .Map(d => d.Person, worker => worker.Person)
            .Map(d => d.Name, worker => worker.Party.Name)
            .Map(d => d.NameAlias, worker => worker.Party.NameAlias)
            .Map(d => d.GenderId, worker => worker.GenderId)
            .Map(d => d.GenderName, worker => worker.Gender.Name)
            .Map(d => d.NationalityId, worker => worker.NationalityId)
            .Map(d => d.NationalityName, worker => worker.Nationality.Name)
            .Map(d => d.HireDate, worker => worker.HireDate)
            .Map(d => d.BirthDate, worker => worker.BirthDate)
            .Map(d => d.UserId, worker => worker.UserId)
            .Map(d => d.IsActive, worker => worker.IsActive)
            .Map(d => d.DataAreaId, worker => worker.DataAreaId);

        config.NewConfig<HcmWorker, HcmWorkerLookupDto>().MapWith(worker => new HcmWorkerLookupDto(
                worker.RecId,
                worker.PersonnelNumber,
                worker.Party.Name,
                worker.Party.NameAlias));

        config.NewConfig<HcmWorkerOrganizationAssignmentV1, HcmWorkerOrganizationAssignmentV1Dto>().MapWith(assignment => new HcmWorkerOrganizationAssignmentV1Dto(
                assignment.RecId,
                assignment.HcmManagerWorkerId,
                assignment.HcmManager.PersonnelNumber,
                assignment.HcmManager.Party.Name,
                assignment.HcmManager.Party.NameAlias,
                assignment.DepartmentId,
                assignment.Department != null ? assignment.Department.Name : null,
                assignment.Department != null ? assignment.Department.NameAlias : null,
                assignment.OccupationId,
                assignment.Occupation != null ? assignment.Occupation.Name : null,
                assignment.Occupation != null ? assignment.Occupation.NameAlias : null,
                assignment.ValidFrom,
                assignment.ValidTo,
                assignment.IsPrimary,
                assignment.IsActive));

        config.NewConfig<IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments.HcmWorkerShowroomAssignment, HcmWorkerShowroomAssignmentDto>().MapWith(assignment => new HcmWorkerShowroomAssignmentDto(
                assignment.RecId,
                assignment.HcmShowroomId,
                assignment.HcmShowroom.PartyTable.Name,
                assignment.HcmShowroom.PartyTable.NameAlias,
                assignment.ValidFrom,
                assignment.ValidTo,
                assignment.IsPrimary,
                assignment.IsActive));

        config.NewConfig<HcmWorker, HcmWorkerAssignmentChainNodeDto>().MapWith(item => new HcmWorkerAssignmentChainNodeDto(
                    "manager",
                    item.RecId,
                    item.PersonnelNumber,
                    item.Party.Name,
                    item.Party.NameAlias,
                    item.WorkerOrganizationAssignmentsV1
                        .Where(assignment => assignment.IsPrimary)
                        .OrderByDescending(assignment => assignment.ValidFrom)
                        .Select(assignment => assignment.Occupation != null ? assignment.Occupation.Name : null)
                        .FirstOrDefault(),
                    item.WorkerOrganizationAssignmentsV1
                        .Where(assignment => assignment.IsPrimary)
                        .OrderByDescending(assignment => assignment.ValidFrom)
                        .Select(assignment => assignment.Occupation != null ? assignment.Occupation.NameAlias : null)
                        .FirstOrDefault()));

        config.NewConfig<IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments.HcmWorkerShowroomAssignment, HcmWorkerAssignmentChainNodeDto>().MapWith(assignment => new HcmWorkerAssignmentChainNodeDto(
                "showroom",
                assignment.HcmShowroomId,
                assignment.HcmShowroomId.ToString(),
                assignment.HcmShowroom.PartyTable.Name,
                assignment.HcmShowroom.PartyTable.NameAlias,
                null,
                null));

        config.NewConfig<SaveHcmWorkerOrganizationAssignmentV1Request, HcmWorkerOrganizationAssignmentV1>().IgnoreNonMapped(true)
            .Map(d => d.HcmManagerWorkerId, s => s.HcmManagerWorkerId)
            .Map(d => d.DepartmentId, s => s.DepartmentId)
            .Map(d => d.OccupationId, s => s.OccupationId)
            .Map(d => d.ValidFrom, s => s.ValidFrom)
            .Map(d => d.ValidTo, s => s.ValidTo)
            .Map(d => d.IsPrimary, s => s.IsPrimary)
            .Map(d => d.IsActive, s => s.IsActive);

        config.NewConfig<SaveHcmWorkerShowroomAssignmentRequest, IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments.HcmWorkerShowroomAssignment>().IgnoreNonMapped(true)
            .Map(d => d.HcmShowroomId, s => s.HcmShowroomId)
            .Map(d => d.ValidFrom, s => s.ValidFrom)
            .Map(d => d.ValidTo, s => s.ValidTo)
            .Map(d => d.IsPrimary, s => s.IsPrimary)
            .Map(d => d.IsActive, s => s.IsActive);
    }
}
