using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    [ApiController]
    [Route("api/v1/TaxAuthorityAddress")]
    [Route("api/TaxAuthorityAddress")]
    [Route("api/v1/TaxAuthority")]
    [Route("api/TaxAuthority")]
    [DomainPermission("Tax", "TaxAuthorities")]
    public class TaxAuthorityAddressController : BaseController<TaxAuthorityAddress, TaxAuthorityAddressDto>
    {
        private readonly IFinanceDataContext _db;

        public TaxAuthorityAddressController(IBaseService<TaxAuthorityAddress> service, IFinanceDataContext db, ILogger<TaxAuthorityAddressController> logger)
            : base(service, logger)
        {
            _db = db;
        }

        protected override Task OnBeforeCreateAsync(TaxAuthorityAddress entity)
        {
            SanitizeEntity(entity);
            return base.OnBeforeCreateAsync(entity);
        }

        protected override Task OnBeforeUpdateAsync(TaxAuthorityAddress entity)
        {
            SanitizeEntity(entity);
            return base.OnBeforeUpdateAsync(entity);
        }

        private static void SanitizeEntity(TaxAuthorityAddress entity)
        {
            entity.TaxAuthority = entity.TaxAuthority?.Trim() ?? string.Empty;
            entity.Name = entity.Name?.Trim() ?? string.Empty;
            entity.TaxAuthorityId = string.IsNullOrWhiteSpace(entity.TaxAuthorityId) ? entity.TaxAuthority : entity.TaxAuthorityId;
            entity.AccountNum ??= string.Empty;
            entity.Phone ??= string.Empty;
            entity.Mobile ??= string.Empty;
            entity.Fax ??= string.Empty;
            entity.Sms ??= string.Empty;
            entity.Telex ??= string.Empty;
            entity.Extension ??= string.Empty;
            entity.Pager ??= string.Empty;
            entity.Email ??= string.Empty;
            entity.Url ??= string.Empty;
            entity.Address ??= string.Empty;
            entity.DataAreaId = string.IsNullOrWhiteSpace(entity.DataAreaId) ? "dat" : entity.DataAreaId;
        }
    }
}

