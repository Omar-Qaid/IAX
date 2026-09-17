using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public interface ICurrencyService : IBaseService<Currency>
    {
    }
}
