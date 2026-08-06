using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Modules.Administration.NumberSequences;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    [ScopedService]
    public class WfActivityTypeService : BaseService<WfActivityType>, IWfActivityTypeService
    {
        private readonly ISysNumberSequenceService _sequences;

        public WfActivityTypeService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ISysNumberSequenceService sequences) : base(unitOfWork, currentUser)
        {
            _sequences = sequences;
        }

        protected override async Task OnBeforeAddAsync(WfActivityType entity, CancellationToken cancellationToken)
        {
            await _sequences.EnsureCodeAsync(entity, entityName: "WfActivityType", cancellationToken: cancellationToken);
        }
    }
}
