using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventJournal")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventJournalController
    : BaseController<InventJournalTable, InventJournalTableDto>
{
    private readonly IInventJournalService _journalService;
    private readonly IInventJournalPostingService _posting;

    public InventJournalController(
        IInventJournalService service,
        IInventJournalPostingService posting,
        ILogger<InventJournalController> logger)
        : base(service, logger)
    {
        _journalService = service;
        _posting = posting;
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventJournalTableDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventJournalTableDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventJournalTableDto>>> Create(InventJournalTableDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventJournalTableDto>>> Update(string id, InventJournalTableDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(APIResponse<object>.Ok(await _journalService.ListAsync(ct)));

    [HttpGet("{journalId}")]
    public async Task<IActionResult> Get(string journalId, CancellationToken ct)
    {
        var row = await _journalService.GetAsync(journalId, ct);
        return row == null ? NotFound(APIResponse<object>.Fail("Inventory journal was not found.")) : Ok(APIResponse<object>.Ok(row));
    }

    [HttpGet("lookups")]
    public async Task<IActionResult> Lookups(CancellationToken ct) =>
        Ok(APIResponse<object>.Ok(await _journalService.LookupsAsync(ct)));

    [HttpGet("ledger-dimensions")]
    public async Task<IActionResult> LedgerDimensions([FromQuery] string? search, CancellationToken ct) =>
        Ok(APIResponse<object>.Ok(await _journalService.LedgerDimensionsAsync(search, ct)));

    [HttpPost]
    public async Task<IActionResult> Create(InventJournalCreateDto input, CancellationToken ct)
    {
        var result = await _journalService.CreateAsync(input, ct);
        return result.Error == null ? Ok(APIResponse<object>.Ok(result.Journal!)) : UnprocessableEntity(APIResponse<object>.Fail(result.Error));
    }

    [HttpPost("{journalId}/lines")]
    public async Task<IActionResult> AddLine(string journalId, InventJournalLineInputDto input, CancellationToken ct)
    {
        var result = await _journalService.AddLineAsync(journalId, input, ct);
        return result.Error == null ? Ok(APIResponse<object>.Ok(result.Journal!)) : UnprocessableEntity(APIResponse<object>.Fail(result.Error));
    }

    [HttpPut("{journalId}/lines/{lineRecId:long}")]
    public async Task<IActionResult> UpdateLine(string journalId, long lineRecId, InventJournalLineInputDto input, CancellationToken ct)
    {
        var result = await _journalService.UpdateLineAsync(journalId, lineRecId, input, ct);
        return result.Error == null ? Ok(APIResponse<object>.Ok(result.Journal!)) : UnprocessableEntity(APIResponse<object>.Fail(result.Error));
    }

    [HttpDelete("{journalId}/lines/{lineRecId:long}")]
    public async Task<IActionResult> DeleteLine(string journalId, long lineRecId, CancellationToken ct)
    {
        var result = await _journalService.DeleteLineAsync(journalId, lineRecId, ct);
        return result.Error == null ? Ok(APIResponse<bool>.Ok(result.Deleted)) : Conflict(APIResponse<object>.Fail(result.Error));
    }

    [HttpDelete("{journalId}")]
    public override async Task<ActionResult<APIResponse<bool>>> Delete(string journalId, CancellationToken cancellationToken = default)
    {
        var result = await _journalService.DeleteAsync(journalId, cancellationToken);
        return result.Error == null ? Ok(APIResponse<bool>.Ok(result.Deleted)) : Conflict(APIResponse<bool>.Fail(result.Error));
    }

    [HttpPost("{journalId}/post")]
    public async Task<IActionResult> Post(string journalId, CancellationToken ct)
    {
        var result = await _posting.PostAsync(journalId, ct);
        return result.Error == null ? Ok(APIResponse<object>.Ok(result.Result!)) : UnprocessableEntity(APIResponse<object>.Fail(result.Error));
    }
}
