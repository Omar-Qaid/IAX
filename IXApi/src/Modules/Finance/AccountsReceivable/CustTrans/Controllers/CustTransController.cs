using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

[ApiController]
[Route("api/v1/[controller]")]
[Route("api/[controller]")]
[DomainPermission("AccountsReceivable", "Transactions", "View")]
public class CustTransController : BaseController<CustTrans, CustTransDto>
{
    public CustTransController(
        ICustTransService service,
        ILogger<CustTransController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<CustTransController>.Instance)
    {
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<CustTransDto>>> Create(CustTransDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<CustTransDto>>> Update(string id, CustTransDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
