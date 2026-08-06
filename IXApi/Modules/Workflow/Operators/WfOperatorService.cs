using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Modules.Administration.NumberSequences;



namespace IAX.IXApi.Modules.Workflow.Operators
{
    [ScopedService]
    public class WfOperatorService : BaseService<WfOperator>, IWfOperatorService
    {
        private readonly ISysNumberSequenceService _sequences;

        public WfOperatorService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ISysNumberSequenceService sequences) : base(unitOfWork, currentUser)
        {
            _sequences = sequences;
        }

        protected override async Task OnBeforeAddAsync(WfOperator entity, CancellationToken cancellationToken)
        {
            await _sequences.EnsureCodeAsync(entity, entityName: "WfOperator", cancellationToken: cancellationToken);
        }
    }
}
