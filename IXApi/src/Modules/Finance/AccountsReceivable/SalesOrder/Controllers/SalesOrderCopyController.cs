using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

[ApiController]
[Route("api/v1/SalesTable/{destinationRecId:long}/copy")]
[DomainPermission("AccountsReceivable", "SalesOrders", "View")]
public sealed class SalesOrderCopyController : BaseController<SalesTable, SalesOrderCopyDto>
{
    private readonly ISalesOrderCopyService _copy;

    public SalesOrderCopyController(
        ISalesOrderCopyService copy,
        ILogger<SalesOrderCopyController>? logger = null)
        : base(copy, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<SalesOrderCopyController>.Instance)
    {
        _copy = copy;
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<SalesOrderCopyDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<SalesOrderCopyDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<SalesOrderCopyDto>>>> GetPaged(QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<SalesOrderCopyDto>>> Create(SalesOrderCopyDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<SalesOrderCopyDto>>> Update(string id, SalesOrderCopyDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    [HttpGet("from-all/documents")]
    public async Task<IActionResult> SalesOrders(long destinationRecId, CancellationToken ct)
        => Present(await _copy.GetSalesOrdersAsync(destinationRecId, ct));

    [HttpGet("from-all/documents/{sourceRecId:long}/lines")]
    public async Task<IActionResult> SalesOrderLines(long destinationRecId, long sourceRecId, CancellationToken ct)
        => Present(await _copy.GetSalesOrderLinesAsync(destinationRecId, sourceRecId, ct));

    [HttpGet("from-journal/documents")]
    public async Task<IActionResult> Journals(long destinationRecId, CancellationToken ct)
        => Present(await _copy.GetJournalsAsync(destinationRecId, ct));

    [HttpGet("from-journal/documents/{sourceRecId:long}/lines")]
    public async Task<IActionResult> JournalLines(long destinationRecId, long sourceRecId, CancellationToken ct)
        => Present(await _copy.GetJournalLinesAsync(destinationRecId, sourceRecId, ct));

    public sealed record CopyRequest(string Mode, List<string> LineIds, decimal QuantityFactor = 1,
        bool InvertSign = false, bool RecalculatePrice = false, bool CopyPrecisely = true)
        : SalesOrderCopyRequest(Mode, LineIds, QuantityFactor, InvertSign, RecalculatePrice, CopyPrecisely);

    [HttpPost]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> Copy(long destinationRecId, [FromBody] CopyRequest request, CancellationToken ct)
    {
        var result = await _copy.CopyAsync(destinationRecId, request, ct);
        return result.Status switch
        {
            SalesOrderCopyStatus.InvalidRequest => BadRequest(APIResponse<object>.Fail(result.Error!)),
            SalesOrderCopyStatus.DestinationNotFound => NotFound(APIResponse<object>.Fail("Destination sales order was not found.")),
            SalesOrderCopyStatus.DestinationClosed => UnprocessableEntity(APIResponse<object>.Fail("Lines can only be copied to an open sales order.")),
            SalesOrderCopyStatus.SourceLinesNotFound => UnprocessableEntity(APIResponse<object>.Fail("One or more selected source lines were not found.")),
            SalesOrderCopyStatus.InvalidUnit => UnprocessableEntity(APIResponse<object>.Fail("A source line has no valid conversion to its inventory unit.")),
            _ => Ok(APIResponse<object>.Ok(new { copiedLineCount = result.CopiedLineCount }))
        };
    }

    private IActionResult Present(SalesOrderCopyLookupResult result) => result.StatusCode switch
    {
        404 => NotFound(APIResponse<object>.Fail(result.Error!)),
        422 => UnprocessableEntity(APIResponse<object>.Fail(result.Error!)),
        _ => Ok(APIResponse<object>.Ok(result.Data!))
    };
}
