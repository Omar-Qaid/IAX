using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Administration.NumberSequences;



namespace IAX.IXApi.Modules.Workflow.Steps
{
    public class WfStepService : BaseService<WfStep>, IWfStepService
    {
        private readonly ISysNumberSequenceService _sequences;

        public WfStepService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ISysNumberSequenceService sequences) : base(unitOfWork, currentUser)
        {
            _sequences = sequences;
        }

        protected override async Task OnBeforeAddAsync(WfStep entity, CancellationToken cancellationToken)
        {
            await _sequences.EnsureCodeAsync(entity, entityName: "WfStep", cancellationToken: cancellationToken);
        }
    }
}

