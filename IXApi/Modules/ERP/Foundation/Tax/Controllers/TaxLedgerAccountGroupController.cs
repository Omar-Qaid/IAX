using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Infrastructure.Persistence.Services;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.ERP.Shared.Features
{
    [ApiController]
    [Route("api/v1/TaxLedgerAccountGroup")]
    [Route("api/TaxLedgerAccountGroup")]
    public class TaxLedgerAccountGroupController : BaseController<TaxLedgerAccountGroup, TaxLedgerAccountGroupDto>
    {
        private readonly ApplicationDbContext _db;

        public TaxLedgerAccountGroupController(IBaseService<TaxLedgerAccountGroup> service, ApplicationDbContext db, ILogger<TaxLedgerAccountGroupController> logger)
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
