using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Application.Identity;
using InventTableEntity = IAX.IXApi.Modules.Finance.Entities.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory
{
    public class InventTableService : BaseService<InventTableEntity>, IInventTableService
    {
        public InventTableService(IUnitOfWork unitOfWork)
            : this(unitOfWork, NullCurrentUserService.Instance)
        {
        }

        public InventTableService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
            : base(unitOfWork, currentUserService)
        {
        }
    }
}
