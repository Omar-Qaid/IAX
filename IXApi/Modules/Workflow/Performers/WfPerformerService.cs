using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Modules.Administration.NumberSequences;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    [ScopedService]
    public class WfPerformerService : BaseService<WfPerformer>, IWfPerformerService
    {
        private readonly ISysNumberSequenceService _sequences;

        public WfPerformerService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ISysNumberSequenceService sequences) : base(unitOfWork, currentUser)
        {
            _sequences = sequences;
        }

        protected override async Task OnBeforeAddAsync(WfPerformer entity, CancellationToken cancellationToken)
        {
            await _sequences.EnsureCodeAsync(entity, entityName: "WfPerformer", cancellationToken: cancellationToken);
        }
    }
}
