using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;

namespace IAX.IXApi.Modules.Finance.Inventory
{
    public class InventTableModuleService : BaseService<InventTableModule>, IInventTableModuleService
    {
        public InventTableModuleService(IUnitOfWork unitOfWork)
            : this(unitOfWork, NullCurrentUserService.Instance)
        {
        }

        public InventTableModuleService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
            : base(unitOfWork, currentUserService)
        {
        }
    }
}
