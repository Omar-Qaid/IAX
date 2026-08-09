using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;
using IAX.IXApi.Modules.Finance.Foundation.LegalEntities;

namespace IAX.IXApi.Modules.Finance.Foundation.LegalEntities
{
    public interface ICompanyInfoService : IBaseService<CompanyInfo>
    {
        Task<CompanyInfo> CreateCompanyWithAddressBookAsync(CompanyInfoDto dto, CancellationToken cancellationToken);
        Task<CompanyInfo> UpdateCompanyWithAddressBookAsync(string id, CompanyInfoDto dto, CancellationToken cancellationToken);
        Task PopulateGlobalAddressBookAsync(IEnumerable<CompanyInfoDto> dtos, CancellationToken cancellationToken);
    }
}

