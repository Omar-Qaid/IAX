using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;

namespace IAX.IXApi.Modules.Finance.Inventory
{
    public class InventItemPurchSetupService : BaseService<InventItemPurchSetup>, IInventItemPurchSetupService
    {
        public InventItemPurchSetupService(IUnitOfWork unitOfWork)
            : this(unitOfWork, NullCurrentUserService.Instance)
        {
        }

        public InventItemPurchSetupService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
            : base(unitOfWork, currentUserService)
        {
        }
    }
}
