using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Foundation.Markup;

[ApiController]
[Route("api/v1/MarkupTrans")]
[DomainPermission("AccountsReceivable", "SalesOrders", "View")]
public sealed class MarkupTransController(IMarkupTransService service) : ControllerBase
{
    [HttpGet("document/{documentType}/{documentRecId:long}")]
    public async Task<IActionResult> GetDocumentCharges(string documentType, long documentRecId,
        [FromQuery] string level = "header", CancellationToken cancellationToken = default) =>
        Respond(await service.GetDocumentChargesAsync(documentType, documentRecId, level, cancellationToken));

    [HttpGet("codes/{documentType}")]
    public async Task<IActionResult> GetChargeCodes(string documentType, CancellationToken cancellationToken = default) =>
        Respond(await service.GetChargeCodesAsync(documentType, cancellationToken));

    [HttpPost("document/{documentType}/{documentRecId:long}")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> Create(string documentType, long documentRecId, [FromBody] MarkupTransDto input,
        [FromQuery] string level = "header", CancellationToken cancellationToken = default) =>
        Respond(await service.CreateAsync(documentType, documentRecId, input, level, cancellationToken));

    [HttpPut("{id:long}")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> Update(long id, [FromBody] MarkupTransDto input, CancellationToken cancellationToken = default) =>
        Respond(await service.UpdateAsync(id, input, cancellationToken));

    [HttpDelete("{id:long}")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken = default) =>
        Respond(await service.DeleteAsync(id, cancellationToken));

    private IActionResult Respond<T>(MarkupResult<T> result)
    {
        if (result.Error is { } error)
            return error.NotFound
                ? NotFound(APIResponse<T>.Fail(error.Message))
                : BadRequest(APIResponse<T>.Fail(error.Message));
        return Ok(APIResponse<T>.Ok(result.Data));
    }
}
