using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Organization.HcmWorkerManagers
{
    public interface IHcmWorkerManagerService : IBaseService<HcmWorkerManager>
    {
        /// <summary>Returns all manager assignments for an employee (with manager + level names).</summary>
        Task<IEnumerable<HcmWorkerManagerDto>> GetForEmployeeAsync(long employeeId, CancellationToken cancellationToken = default);

        /// <summary>Returns every manager assignment org-wide (for the hierarchy diagram).</summary>
        Task<IEnumerable<HcmWorkerManagerDto>> GetAllAssignmentsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces the full set of manager assignments for an employee: rows not present in
        /// <paramref name="rows"/> are deleted, the rest are upserted. Returns the resulting set.
        /// </summary>
        Task<IEnumerable<HcmWorkerManagerDto>> ReplaceForEmployeeAsync(long employeeId, IEnumerable<HcmWorkerManagerDto> rows, CancellationToken cancellationToken = default);

        /// <summary>Returns the direct reports of a manager (employees who report to them at any level).</summary>
        Task<IEnumerable<HcmWorkerManagerDto>> GetReportsAsync(long managerId, CancellationToken cancellationToken = default);
    }
}

