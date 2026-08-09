using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
{
    public interface ILocationService : IBaseService<LogisticsLocation>
    {
        Task<LogisticsLocation> CreateLocationAsync(string description, bool isPostalAddress, CancellationToken cancellationToken = default);
        Task<LogisticsLocation> UpdateLocationDescriptionAsync(long locationRecId, string description, CancellationToken cancellationToken = default);
        Task<bool> DeleteLocationAsync(long locationRecId, CancellationToken cancellationToken = default);
    }
}

