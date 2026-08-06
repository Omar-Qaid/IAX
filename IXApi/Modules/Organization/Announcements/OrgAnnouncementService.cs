using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;

namespace IAX.IXApi.Modules.Organization.Announcements
{
    public class OrgAnnouncementService : BaseService<OrgAnnouncement>, IOrgAnnouncementService
    {
        public OrgAnnouncementService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }
    }
}

