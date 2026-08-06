using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Administration.NumberSequences;



namespace IAX.IXApi.Modules.Workflow.Variables
{
    [ScopedService]
    public class WfVariableService : BaseService<WfVariable>, IWfVariableService
    {
        private readonly ISysNumberSequenceService _sequences;

        public WfVariableService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ISysNumberSequenceService sequences) : base(unitOfWork, currentUser)
        {
            _sequences = sequences;
        }

        protected override async Task OnBeforeAddAsync(WfVariable entity, CancellationToken cancellationToken)
        {
            await _sequences.EnsureCodeAsync(entity, entityName: "WfVariable", cancellationToken: cancellationToken);
        }
    }
}
