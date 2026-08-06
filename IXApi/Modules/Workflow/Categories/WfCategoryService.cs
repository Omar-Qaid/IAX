using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Modules.Administration.NumberSequences;

namespace IAX.IXApi.Modules.Workflow.Categories
{
    public class WfCategoryService : BaseService<WfCategory>, IWfCategoryService
    {
        private readonly ISysNumberSequenceService _sequences;

        public WfCategoryService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ISysNumberSequenceService sequences) : base(unitOfWork, currentUser)
        {
            _sequences = sequences;
        }

        protected override async Task OnBeforeAddAsync(WfCategory entity, CancellationToken cancellationToken)
        {
            await _sequences.EnsureCodeAsync(entity, entityName: "WfCategory", cancellationToken: cancellationToken);
        }
    }
}

