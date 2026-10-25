using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;

public class OrganizationUnitService : BaseService<OrganizationUnit>, IOrganizationUnitService
{
    public OrganizationUnitService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
        : base(unitOfWork, currentUser)
    {
    }
}
