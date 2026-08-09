using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
{
    public interface IPartyService : IBaseService<DirPartyTable>
    {
        Task<DirPartyTable> CreatePartyAsync(string name, string languageId, CancellationToken cancellationToken = default);
        Task<DirPartyTable> UpdatePartyNameAsync(long partyRecId, string name, CancellationToken cancellationToken = default);
    }
}

