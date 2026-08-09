using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public interface IExchangeRateCurrencyPairService : IBaseService<ExchangeRateCurrencyPair>
    {
        Task<BulkExchangeRatePairDto> BulkSaveAsync(BulkExchangeRatePairDto dto);
    }
}
