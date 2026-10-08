using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax.Controllers;

[ApiController]
[Route("api/v1/TaxAuthorityAddress")]
[Route("api/TaxAuthorityAddress")]
[DomainPermission("Tax", "TaxAuthorities")]
public sealed class TaxAuthorityAddressController
    : BaseController<TaxAuthorityAddress, TaxAuthorityAddressDto>
{
    public TaxAuthorityAddressController(
        ITaxAuthorityAddressService service,
        ILogger<TaxAuthorityAddressController> logger)
        : base(service, logger)
    {
    }

}
