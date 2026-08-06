using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Administration.NumberSequences;



namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestService : BaseService<WfRequest>, IWfRequestService
    {
        private readonly ISysNumberSequenceService _sequences;

        public WfRequestService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ISysNumberSequenceService sequences) : base(unitOfWork, currentUser)
        {
            _sequences = sequences;
        }

        protected override async Task OnBeforeAddAsync(WfRequest entity, CancellationToken cancellationToken)
        {
            await _sequences.EnsureCodeAsync(entity, entityName: "WfRequest", cancellationToken: cancellationToken);
        }
    }
}

