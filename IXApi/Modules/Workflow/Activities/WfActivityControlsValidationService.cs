using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    [ScopedService]
    public class WfActivityControlsValidationService : BaseService<WfActivityControlsValidation>, IWfActivityControlsValidationService
    {
        public WfActivityControlsValidationService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }
    }
}
