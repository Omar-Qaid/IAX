using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;

namespace IAX.IXApi.Modules.Finance.Foundation.Occupations
{
    public class HcmOccupationService : BaseService<HcmOccupation>, IHcmOccupationService
    {
        public HcmOccupationService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }
    }
}
