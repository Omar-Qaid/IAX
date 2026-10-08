using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Foundation.Markup;

[ApiController]
[Route("api/v1/MarkupTable")]
[DomainPermission("GeneralLedger", "ChargesCodes")]
public sealed class MarkupTableController : BaseController<MarkupTable, MarkupTableDto>
{
    public MarkupTableController(IMarkupTableService service, ILogger<MarkupTableController> logger)
        : base(service, logger)
    {
    }
}
