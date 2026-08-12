using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;

namespace IAX.IXApi.Modules.Workflow.ProcessTypes
{
    public class WfProcessTypeService : BaseService<WfProcessType>, IWfProcessTypeService
    {
        private readonly ISysNumberSequenceService _sequences;

        public WfProcessTypeService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ISysNumberSequenceService sequences)
            : base(unitOfWork, currentUser)
        {
            _sequences = sequences;
        }

        protected override async Task OnBeforeAddAsync(WfProcessType entity, CancellationToken cancellationToken)
        {
            await _sequences.EnsureCodeAsync(entity, entityName: "WfProcessType", cancellationToken: cancellationToken);
        }
    }
}
