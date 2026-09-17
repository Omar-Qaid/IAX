using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;

namespace IAX.IXApi.Modules.Organization.ManagementLevels
{
    public class HcmWorkerManagementLevelService : BaseService<HcmWorkerManagementLevel>, IHcmWorkerManagementLevelService
    {
        public HcmWorkerManagementLevelService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }
    }
}
