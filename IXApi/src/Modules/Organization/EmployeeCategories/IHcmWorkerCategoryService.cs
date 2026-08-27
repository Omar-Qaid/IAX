using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Organization.Features.HcmWorkerCategory
{
    public interface IHcmWorkerCategoryService : IBaseService<HcmWorkerCategory>
    {
        /// <summary>Load a category with its linkage Groups.</summary>
        Task<HcmWorkerCategory?> GetWithGroupsAsync(long id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update the category scalars and reconcile its Groups collection
        /// (add new, update matched by id, delete removed). When <paramref name="groups"/>
        /// is null the Groups collection is left untouched.
        /// </summary>
        Task<HcmWorkerCategory?> UpdateWithGroupsAsync(long id, HcmWorkerCategory scalars, List<HcmWorkerCategoryGroup>? groups, CancellationToken cancellationToken = default);
    }
}

