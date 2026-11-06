using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [DomainPermission("AccountsPayable", "Vendors")]
    public class VendBankAccountController : BaseController<VendBankAccount, VendBankAccountDto>
    {
        public VendBankAccountController(IVendBankAccountService service, ILogger<VendBankAccountController> logger)
            : base(service, logger)
        {
        }
    }
}
