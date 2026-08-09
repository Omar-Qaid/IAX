using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;

namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeGroup
{
    public class OrgEmployeeGroupService : BaseService<OrgEmployeeGroup>, IOrgEmployeeGroupService
    {
        public OrgEmployeeGroupService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }
    }
}


