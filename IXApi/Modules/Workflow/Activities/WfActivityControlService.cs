using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Shared.Application.Attributes;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityControlService : BaseService<WfActivityControl>, IWfActivityControlService
    {
        public WfActivityControlService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }
    }
}

