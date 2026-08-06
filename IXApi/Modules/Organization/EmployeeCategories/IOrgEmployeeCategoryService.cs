using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeCategory
{
    public interface IOrgEmployeeCategoryService : IBaseService<OrgEmployeeCategory>
    {
        /// <summary>Load a category with its linkage Groups.</summary>
        Task<OrgEmployeeCategory?> GetWithGroupsAsync(long id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update the category scalars and reconcile its Groups collection
        /// (add new, update matched by id, delete removed). When <paramref name="groups"/>
        /// is null the Groups collection is left untouched.
        /// </summary>
        Task<OrgEmployeeCategory?> UpdateWithGroupsAsync(long id, OrgEmployeeCategory scalars, List<OrgEmployeeCategoryGroup>? groups, CancellationToken cancellationToken = default);
    }
}

