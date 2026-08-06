using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.ERP.Foundation.LogisticsAddresses
{
    public interface IPartyService : IBaseService<DirPartyTable>
    {
        Task<DirPartyTable> CreatePartyAsync(string name, string languageId, CancellationToken cancellationToken = default);
        Task<DirPartyTable> UpdatePartyNameAsync(long partyRecId, string name, CancellationToken cancellationToken = default);
    }
}
