using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;

namespace IAX.IXApi.Modules.Finance.Inventory
{
    public class InventSettlementService : BaseService<InventSettlement>, IInventSettlementService
    {
        public InventSettlementService(IUnitOfWork unitOfWork)
            : this(unitOfWork, NullCurrentUserService.Instance)
        {
        }

        public InventSettlementService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
            : base(unitOfWork, currentUserService)
        {
        }
    }
}
