using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    [ScopedService]
    public class WfRequestControlsValidationService : BaseService<WfRequestControlsValidation>, IWfRequestControlsValidationService
    {
        public WfRequestControlsValidationService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }
    }
}
