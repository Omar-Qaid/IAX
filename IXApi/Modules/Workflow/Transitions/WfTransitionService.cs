using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Modules.Administration.NumberSequences;



namespace IAX.IXApi.Modules.Workflow.Transitions
{
    [ScopedService]
    public class WfTransitionService : BaseService<WfTransition>, IWfTransitionService
    {
        private readonly ISysNumberSequenceService _sequences;

        public WfTransitionService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ISysNumberSequenceService sequences) : base(unitOfWork, currentUser)
        {
            _sequences = sequences;
        }

        protected override async Task OnBeforeAddAsync(WfTransition entity, CancellationToken cancellationToken)
        {
            await _sequences.EnsureCodeAsync(entity, entityName: "WfTransition", cancellationToken: cancellationToken);
        }
    }
}
