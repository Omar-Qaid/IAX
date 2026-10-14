using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;

[ApiController]
[Route("api/v1/SalesTable/{salesOrderRecId:long}/packing-slips")]
[DomainPermission("AccountsReceivable", "SalesOrders", "View")]
public sealed class PackingSlipJournalController(IPackingSlipJournalService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(long salesOrderRecId, CancellationToken ct) =>
        Present(await service.ListAsync(salesOrderRecId, ct));

    [HttpGet("posting-preview")]
    public async Task<IActionResult> PreviewPosting(long salesOrderRecId, [FromQuery] string quantityMode = "all",
        CancellationToken ct = default) =>
        Present(await service.PreviewPostingAsync(salesOrderRecId, quantityMode, ct));

    [HttpGet("{packingSlipRecId:long}")]
    public async Task<IActionResult> Get(long salesOrderRecId, long packingSlipRecId, CancellationToken ct) =>
        Present(await service.GetAsync(salesOrderRecId, packingSlipRecId, ct));

    [HttpPost]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> Post(long salesOrderRecId, [FromBody] PostPackingSlipRequest? request,
        CancellationToken ct) => Present(await service.PostAsync(salesOrderRecId, request, ct));

    private IActionResult Present(PackingSlipJournalResult result) => result.StatusCode switch
    {
        200 => Ok(APIResponse<object>.Ok(result.Data!, result.Message!)),
        400 => BadRequest(APIResponse<object>.Fail(result.Message!)),
        404 => NotFound(APIResponse<object>.Fail(result.Message!)),
        422 => UnprocessableEntity(APIResponse<object>.Fail(result.Message!)),
        _ => StatusCode(result.StatusCode, APIResponse<object>.Fail(result.Message ?? "Packing slip request failed."))
    };
}
