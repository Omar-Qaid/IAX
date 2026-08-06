using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.ERP.Shared.Features
{
    public interface ICurrencyService : IBaseService<Currency>
    {
    }

    public interface IExchangeRateService : IBaseService<ExchangeRate>
    {
    }

    public interface IExchangeRateCurrencyPairService : IBaseService<ExchangeRateCurrencyPair>
    {
        Task<BulkExchangeRatePairDto> BulkSaveAsync(BulkExchangeRatePairDto dto);
    }

    public interface IExchangeRateTypeService : IBaseService<ExchangeRateType>
    {
    }
}
