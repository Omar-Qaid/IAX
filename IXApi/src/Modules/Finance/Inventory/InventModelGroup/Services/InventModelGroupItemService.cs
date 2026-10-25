using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;

namespace IAX.IXApi.Modules.Finance.Inventory
{
    public class InventModelGroupItemService : BaseService<InventModelGroupItem>, IInventModelGroupItemService
    {
        public InventModelGroupItemService(IUnitOfWork unitOfWork)
            : this(unitOfWork, NullCurrentUserService.Instance)
        {
        }

        public InventModelGroupItemService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
            : base(unitOfWork, currentUserService)
        {
        }
    }
}
