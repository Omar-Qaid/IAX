using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;

public class ContactPersonService : BaseService<ContactPerson>, IContactPersonService
{
    public ContactPersonService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
        : base(unitOfWork, currentUser)
    {
    }
}
