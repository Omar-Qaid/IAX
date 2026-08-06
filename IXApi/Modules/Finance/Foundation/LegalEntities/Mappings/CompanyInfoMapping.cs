using Mapster;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Shared.Application.Contracts;
using System;

namespace IAX.IXApi.Modules.Finance.Foundation.LegalEntities
{
    public class CompanyInfoMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CompanyInfo, CompanyInfoDto>()
                .Map(dest => dest.Logo, src => src.Logo != null && src.Logo.Length > 0 ? Convert.ToBase64String(src.Logo) : null)
                .Map(dest => dest.ReportLogo, src => src.ReportLogo != null && src.ReportLogo.Length > 0 ? Convert.ToBase64String(src.ReportLogo) : null);

            config.NewConfig<CompanyInfoDto, CompanyInfo>()
                .Map(dest => dest.Logo, src => string.IsNullOrWhiteSpace(src.Logo) ? null : Convert.FromBase64String(src.Logo))
                .Map(dest => dest.ReportLogo, src => string.IsNullOrWhiteSpace(src.ReportLogo) ? null : Convert.FromBase64String(src.ReportLogo));
        }
    }
}

