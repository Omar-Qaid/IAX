using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Interfaces;

namespace IAX.IXApi.Modules.Communication.Notifications.Services
{
    public class SysNotificationTemplateService : BaseService<SysNotificationTemplate>, ISysNotificationTemplateService
    {
        public SysNotificationTemplateService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }
    }
}
