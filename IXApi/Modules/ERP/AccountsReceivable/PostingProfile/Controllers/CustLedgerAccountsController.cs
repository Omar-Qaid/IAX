using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Services;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    public class CustLedgerAccountsController : BaseController<CustLedgerAccounts, CustLedgerAccountsDto>
    {
        public CustLedgerAccountsController(IBaseService<CustLedgerAccounts> service, ILogger<CustLedgerAccountsController> logger)
            : base(service, logger)
        {
        }

        /// <summary>
        /// Gets all posting profile accounts for a specific profile (e.g., GEN, PrePayment, EXPORT, GOV).
        /// </summary>
        [HttpGet("profile/{postingProfile}")]
        public async Task<ActionResult<APIResponse<IEnumerable<CustLedgerAccountsDto>>>> GetByPostingProfile(string postingProfile, CancellationToken ct = default)
        {
            _logger.LogInformation("[CustLedgerAccounts] - Fetching accounts for PostingProfile: {PostingProfile}", postingProfile);

            var accounts = await _service.FindAsync(a => a.PostingProfile == postingProfile, cancellationToken: ct);
            var dtos = accounts.Adapt<List<CustLedgerAccountsDto>>();

            return Ok(APIResponse<IEnumerable<CustLedgerAccountsDto>>.Ok(dtos));
        }
    }
}
