using IAX.IXApi.Infrastructure.Persistence.Services;
using InventTableEntity = IAX.IXApi.Modules.Finance.Entities.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory
{
    public interface IInventTableService : IBaseService<InventTableEntity>
    {
    }
}
