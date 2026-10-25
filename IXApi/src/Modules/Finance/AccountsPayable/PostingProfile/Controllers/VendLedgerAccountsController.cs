using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

[ApiController]
[Route("api/v1/[controller]")]
[Route("api/[controller]")]
[DomainPermission("AccountsPayable", "PostingProfiles")]
public class VendLedgerAccountsController : BaseController<VendLedgerAccounts, VendLedgerAccountsDto>
{
    public VendLedgerAccountsController(
        IVendLedgerAccountsService service,
        ILogger<VendLedgerAccountsController> logger)
        : base(service, logger)
    {
    }
}
