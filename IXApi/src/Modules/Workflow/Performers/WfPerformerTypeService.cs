using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    public class WfPerformerTypeService : BaseService<WfPerformerType>, IWfPerformerTypeService
    {
        private readonly ISysNumberSequenceService _sequences;

        public WfPerformerTypeService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ISysNumberSequenceService sequences)
            : base(unitOfWork, currentUser)
        {
            _sequences = sequences;
        }

        protected override async Task OnBeforeAddAsync(WfPerformerType entity, CancellationToken cancellationToken)
        {
            await _sequences.EnsureCodeAsync(entity, entityName: "WfPerformerType", cancellationToken: cancellationToken);
        }
    }
}
