using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Organization.EmployeeManagers
{
    [ScopedService]
    public class OrgEmployeeManagerService : BaseService<OrgEmployeeManager>, IOrgEmployeeManagerService
    {
        public OrgEmployeeManagerService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }

        public async Task<IEnumerable<OrgEmployeeManagerDto>> GetForEmployeeAsync(long employeeId, CancellationToken cancellationToken = default)
        {
            return await _repository.GetQueryable()
                .AsNoTracking()
                .Where(x => x.EmployeeId == employeeId)
                .OrderBy(x => x.ManagementLevelId)
                .Select(x => new OrgEmployeeManagerDto
                {
                    EmployeeId = x.EmployeeId,
                    ManagementLevelId = x.ManagementLevelId,
                    ManagerId = x.ManagerId,
                    EmployeeName = x.Employee.PersonnelNumber,
                    ManagerName = x.Manager.PersonnelNumber,
                    ManagementLevelName = x.ManagementLevel.Name
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<OrgEmployeeManagerDto>> GetAllAssignmentsAsync(CancellationToken cancellationToken = default)
        {
            return await _repository.GetQueryable()
                .AsNoTracking()
                .Select(x => new OrgEmployeeManagerDto
                {
                    EmployeeId = x.EmployeeId,
                    ManagementLevelId = x.ManagementLevelId,
                    ManagerId = x.ManagerId,
                    EmployeeName = x.Employee.PersonnelNumber,
                    ManagerName = x.Manager.PersonnelNumber,
                    ManagementLevelName = x.ManagementLevel.Name
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<OrgEmployeeManagerDto>> GetReportsAsync(long managerId, CancellationToken cancellationToken = default)
        {
            return await _repository.GetQueryable()
                .AsNoTracking()
                .Where(x => x.ManagerId == managerId)
                .OrderBy(x => x.ManagementLevelId)
                .ThenBy(x => x.Employee.PersonnelNumber)
                .Select(x => new OrgEmployeeManagerDto
                {
                    EmployeeId = x.EmployeeId,
                    ManagementLevelId = x.ManagementLevelId,
                    ManagerId = x.ManagerId,
                    EmployeeName = x.Employee.PersonnelNumber,
                    ManagerName = x.Manager.PersonnelNumber,
                    ManagementLevelName = x.ManagementLevel.Name
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<OrgEmployeeManagerDto>> ReplaceForEmployeeAsync(long employeeId, IEnumerable<OrgEmployeeManagerDto> rows, CancellationToken cancellationToken = default)
        {
            // Keep the last row per level and drop self-references / empty managers.
            var desired = rows
                .Where(r => r.ManagementLevelId > 0 && r.ManagerId > 0 && r.ManagerId != employeeId)
                .GroupBy(r => r.ManagementLevelId)
                .ToDictionary(g => g.Key, g => g.Last().ManagerId);

            // Reject assignments that would create a management cycle (a chosen manager must not
            // already report — directly or transitively — to this employee).
            await EnsureNoCycleAsync(employeeId, desired.Values.Distinct(), cancellationToken);

            var existing = await _repository.GetQueryable()
                .Where(x => x.EmployeeId == employeeId)
                .ToListAsync(cancellationToken);

            // Delete rows whose level is no longer desired.
            var toRemove = existing.Where(e => !desired.ContainsKey(e.ManagementLevelId)).ToList();
            if (toRemove.Count > 0)
                await _repository.RemoveRangeAsync(toRemove);

            foreach (var (levelId, managerId) in desired)
            {
                var current = existing.FirstOrDefault(e => e.ManagementLevelId == levelId);
                if (current == null)
                {
                    await _repository.AddAsync(new OrgEmployeeManager
                    {
                        EmployeeId = employeeId,
                        ManagementLevelId = levelId,
                        ManagerId = managerId
                    }, cancellationToken);
                }
                else if (current.ManagerId != managerId)
                {
                    current.ManagerId = managerId;
                    await _repository.UpdateAsync(current);
                }
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
            return await GetForEmployeeAsync(employeeId, cancellationToken);
        }

        /// <summary>
        /// Throws a validation error if assigning any of <paramref name="managerIds"/> to
        /// <paramref name="employeeId"/> would close a management cycle — i.e. the employee is
        /// already (transitively) a manager of that person.
        /// </summary>
        private async Task EnsureNoCycleAsync(long employeeId, IEnumerable<long> managerIds, CancellationToken cancellationToken)
        {
            var managerIdList = managerIds.ToList();
            if (managerIdList.Count == 0)
                return;

            // employee -> manager edges for everyone EXCEPT this employee's own rows (being replaced).
            var edges = await _repository.GetQueryable()
                .AsNoTracking()
                .Where(x => x.EmployeeId != employeeId)
                .Select(x => new { x.EmployeeId, x.ManagerId })
                .ToListAsync(cancellationToken);

            var managersOf = edges
                .GroupBy(e => e.EmployeeId)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ManagerId).ToList());

            foreach (var managerId in managerIdList)
            {
                // Walk managerId's upline; if we reach employeeId, employeeId -> managerId is a cycle.
                var stack = new Stack<long>();
                var visited = new HashSet<long>();
                stack.Push(managerId);
                while (stack.Count > 0)
                {
                    var current = stack.Pop();
                    if (!visited.Add(current))
                        continue;
                    if (current == employeeId)
                        throw new FluentValidation.ValidationException(new[]
                        {
                            new FluentValidation.Results.ValidationFailure(
                                nameof(OrgEmployeeManagerDto.ManagerId),
                                "This assignment would create a management cycle: the selected manager already reports to this employee.")
                        });
                    if (managersOf.TryGetValue(current, out var ups))
                        foreach (var up in ups)
                            stack.Push(up);
                }
            }
        }
    }
}

