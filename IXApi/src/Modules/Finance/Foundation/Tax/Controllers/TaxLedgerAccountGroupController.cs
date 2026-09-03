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
    [Route("api/v1/TaxLedgerAccountGroup")]
    [Route("api/TaxLedgerAccountGroup")]
    [DomainPermission("Tax", "LedgerAccountGroups")]
    public class TaxLedgerAccountGroupController : BaseController<TaxLedgerAccountGroup, TaxLedgerAccountGroupDto>
    {
        private readonly IFinanceDataContext _db;

        public TaxLedgerAccountGroupController(IBaseService<TaxLedgerAccountGroup> service, IFinanceDataContext db, ILogger<TaxLedgerAccountGroupController> logger)
            : base(service, logger)
        {
            _db = db;
        }

        protected override Task OnBeforeCreateAsync(TaxLedgerAccountGroup entity)
        {
            SanitizeEntity(entity);
            return base.OnBeforeCreateAsync(entity);
        }

        protected override Task OnBeforeUpdateAsync(TaxLedgerAccountGroup entity)
        {
            SanitizeEntity(entity);
            return base.OnBeforeUpdateAsync(entity);
        }

        private static void SanitizeEntity(TaxLedgerAccountGroup entity)
        {
            entity.TaxAccountGroup = entity.TaxAccountGroup?.Trim() ?? string.Empty;
            entity.Name = entity.Name?.Trim() ?? string.Empty;
            entity.DataAreaId = string.IsNullOrWhiteSpace(entity.DataAreaId) ? "dat" : entity.DataAreaId;
        }
    }
}

