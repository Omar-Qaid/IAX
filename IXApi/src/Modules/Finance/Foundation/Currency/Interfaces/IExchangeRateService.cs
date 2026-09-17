using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.HcmWorkers;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public interface IExchangeRateService : IBaseService<ExchangeRate>
    {
    }
}
