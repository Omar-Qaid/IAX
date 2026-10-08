using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax
{
    [ApiController]
    [Route("api/v1/TaxLedgerAccountGroup")]
    [Route("api/TaxLedgerAccountGroup")]
    [DomainPermission("Tax", "LedgerAccountGroups")]
    public class TaxLedgerAccountGroupController : BaseController<TaxLedgerAccountGroup, TaxLedgerAccountGroupDto>
    {
        private readonly ITaxLedgerAccountGroupService _ledgerService;

        public TaxLedgerAccountGroupController(ITaxLedgerAccountGroupService service, ILogger<TaxLedgerAccountGroupController> logger)
            : base(service, logger)
        {
            _ledgerService = service;
        }

        protected override Task OnBeforeCreateAsync(TaxLedgerAccountGroup entity)
        {
            _ledgerService.Normalize(entity);
            return base.OnBeforeCreateAsync(entity);
        }

        protected override Task OnBeforeUpdateAsync(TaxLedgerAccountGroup entity)
        {
            _ledgerService.Normalize(entity);
            return base.OnBeforeUpdateAsync(entity);
        }

    }
}
