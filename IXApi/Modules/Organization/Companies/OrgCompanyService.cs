using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;

namespace IAX.IXApi.Modules.Organization.Companies
{
    public class OrgCompanyService : BaseService<OrgCompany>, IOrgCompanyService
    {
        public OrgCompanyService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }
    }
}

