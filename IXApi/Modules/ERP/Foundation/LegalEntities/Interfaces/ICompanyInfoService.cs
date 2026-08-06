using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.ERP.Foundation.LogisticsAddresses;
using IAX.IXApi.Modules.ERP.Foundation.LegalEntities;

namespace IAX.IXApi.Modules.ERP.Foundation.LegalEntities
{
    public interface ICompanyInfoService : IBaseService<CompanyInfo>
    {
        Task<CompanyInfo> CreateCompanyWithAddressBookAsync(CompanyInfoDto dto, CancellationToken cancellationToken);
        Task<CompanyInfo> UpdateCompanyWithAddressBookAsync(string id, CompanyInfoDto dto, CancellationToken cancellationToken);
        Task PopulateGlobalAddressBookAsync(IEnumerable<CompanyInfoDto> dtos, CancellationToken cancellationToken);
    }
}
