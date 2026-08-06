using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeCategory
{
    public class OrgEmployeeCategoryService : BaseService<OrgEmployeeCategory>, IOrgEmployeeCategoryService
    {
        public OrgEmployeeCategoryService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }

        public Task<OrgEmployeeCategory?> GetWithGroupsAsync(long id, CancellationToken cancellationToken = default)
            => GetByIdAsync(x => x.RecId == id, q => q.Include(c => c.Groups), asNoTracking: true, cancellationToken);

        public async Task<OrgEmployeeCategory?> UpdateWithGroupsAsync(long id, OrgEmployeeCategory scalars, List<OrgEmployeeCategoryGroup>? groups, CancellationToken cancellationToken = default)
        {
            var existing = await _repository.GetQueryable()
                .Include(c => c.Groups)
                .FirstOrDefaultAsync(c => c.RecId == id, cancellationToken);
            if (existing == null) return null;

            // Scalars
            existing.Code = scalars.Code;
            existing.Name = scalars.Name;
            existing.NameAR = scalars.NameAR;
            existing.Description = scalars.Description;
            existing.DescriptionAR = scalars.DescriptionAR;
            existing.IsActive = scalars.IsActive;
            existing.ForAll = scalars.ForAll;
            existing.Manager1 = scalars.Manager1;
            existing.Manager2 = scalars.Manager2;
            existing.Manager3 = scalars.Manager3;
            existing.Manager4 = scalars.Manager4;

            // Reconcile child rows only when the caller actually sent them.
            if (groups != null)
            {
                // Delete rows no longer present.
                var removed = existing.Groups.Where(g => groups.All(ig => ig.RecId != g.RecId)).ToList();
                foreach (var r in removed) existing.Groups.Remove(r);

                // Add new / update matched.
                foreach (var ig in groups)
                {
                    var match = ig.RecId != 0 ? existing.Groups.FirstOrDefault(g => g.RecId == ig.RecId) : null;
                    if (match != null)
                    {
                        match.DepartmentID = ig.DepartmentID;
                        match.OccupationID = ig.OccupationID;
                        match.UserGroupID = ig.UserGroupID;
                    }
                    else
                    {
                        existing.Groups.Add(new OrgEmployeeCategoryGroup
                        {
                            DepartmentID = ig.DepartmentID,
                            OccupationID = ig.OccupationID,
                            UserGroupID = ig.UserGroupID,
                        });
                    }
                }
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
            return existing;
        }
    }
}



