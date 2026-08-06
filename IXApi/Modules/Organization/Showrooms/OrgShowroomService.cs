using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Organization.Employees;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Organization.Showrooms
{
    [ScopedService]
    public class OrgShowroomService : BaseService<OrgShowroom>, IOrgShowroomService
    {
        private readonly IGenericRepository<IAX.IXApi.Modules.Organization.Employees.Entities.HcmWorker> _employees;

        public OrgShowroomService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
            _employees = unitOfWork.Repository<IAX.IXApi.Modules.Organization.Employees.Entities.HcmWorker>();
        }

        public async Task<IEnumerable<ShowroomSellerDto>> GetSellersAsync(long showroomId, CancellationToken cancellationToken = default)
        {
            return await _employees.GetQueryable()
                .AsNoTracking()
                .Where(e => e.ShowroomId == showroomId)
                .OrderBy(e => e.PersonnelNumber)
                .Select(e => new ShowroomSellerDto
                {
                    RecId = e.RecId,
                    Code = e.PersonnelNumber,
                    Name = e.PersonnelNumber,
                    OccupationName = e.Occupation.Name,
                    Mobile = null, // Retrieved via Address Book
                    Email = null, // Retrieved via Address Book
                    IsActive = e.IsActive
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ShowroomSellerDto>> SetSellersAsync(long showroomId, IEnumerable<long> employeeIds, CancellationToken cancellationToken = default)
        {
            var desired = employeeIds?.Distinct().ToHashSet() ?? new HashSet<long>();

            // Currently-assigned sellers plus any newly requested employees (tracked for update).
            var affected = await _employees.GetQueryable()
                .Where(e => e.ShowroomId == showroomId || desired.Contains(e.RecId))
                .ToListAsync(cancellationToken);

            foreach (var emp in affected)
            {
                var shouldBeSeller = desired.Contains(emp.RecId);
                var newShowroomId = shouldBeSeller ? showroomId : (long?)null;

                // Only unassign if the employee currently belongs to THIS showroom.
                if (!shouldBeSeller && emp.ShowroomId != showroomId)
                    continue;

                if (emp.ShowroomId != newShowroomId)
                {
                    emp.ShowroomId = newShowroomId;
                    await _employees.UpdateAsync(emp);
                }
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
            return await GetSellersAsync(showroomId, cancellationToken);
        }
    }
}


