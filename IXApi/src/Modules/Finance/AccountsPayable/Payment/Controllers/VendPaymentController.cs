using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

[ApiController]
[Route("api/v1/vendor-payments")]
[DomainPermission("AccountsPayable", "VendorPayments", "View")]
public sealed class VendPaymentController : BaseController<VendTrans, VendPaymentPostDto>
{
    private readonly IVendPaymentService _paymentService;

    public VendPaymentController(
        IVendPaymentService service,
        ILogger<VendPaymentController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<VendPaymentController>.Instance)
    {
        _paymentService = service;
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<VendPaymentPostDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<VendPaymentPostDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<VendPaymentPostDto>>>> GetPaged(QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<VendPaymentPostDto>>> Create(VendPaymentPostDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<VendPaymentPostDto>>> Update(string id, VendPaymentPostDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    [HttpPost]
    [DomainPermission("AccountsPayable", "VendorPayments", "Post")]
    public async Task<IActionResult> Post([FromBody] VendPaymentPostDto request, CancellationToken ct)
    {
        var result = await _paymentService.PostAsync(request, ct);
        return result.StatusCode switch
        {
            200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message ?? string.Empty)),
            400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
            404 => NotFound(APIResponse<object>.Fail(result.Message!)),
            _ => UnprocessableEntity(APIResponse<object>.Fail(result.Message ?? "Vendor payment posting failed."))
        };
    }
}
