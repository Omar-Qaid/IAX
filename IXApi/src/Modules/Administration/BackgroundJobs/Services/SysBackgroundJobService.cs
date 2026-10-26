using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services
{
    public class SysBackgroundJobService : BaseService<SysBackgroundJob>, ISysBackgroundJobService
    {
        public SysBackgroundJobService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }
    }
}
