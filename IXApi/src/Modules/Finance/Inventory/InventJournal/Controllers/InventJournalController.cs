using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventJournal")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventJournalController : ControllerBase
{
    private readonly IInventJournalService _service;
    private readonly IInventJournalPostingService _posting;
    public InventJournalController(IInventJournalService service, IInventJournalPostingService posting)
    { _service = service; _posting = posting; }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(APIResponse<object>.Ok(await _service.ListAsync(ct)));

    [HttpGet("{journalId}")]
    public async Task<IActionResult> Get(string journalId, CancellationToken ct)
    {
        var row = await _service.GetAsync(journalId, ct);
        return row == null ? NotFound(APIResponse<object>.Fail("Inventory journal was not found.")) : Ok(APIResponse<object>.Ok(row));
    }

    [HttpGet("lookups")]
    public async Task<IActionResult> Lookups(CancellationToken ct) => Ok(APIResponse<object>.Ok(await _service.LookupsAsync(ct)));

    [HttpPost]
    public async Task<IActionResult> Create(InventJournalCreateDto input, CancellationToken ct)
    {
        var result = await _service.CreateAsync(input, ct);
        return result.Error == null ? Ok(APIResponse<object>.Ok(result.Journal!)) : UnprocessableEntity(APIResponse<object>.Fail(result.Error));
    }

    [HttpPost("{journalId}/lines")]
    public async Task<IActionResult> AddLine(string journalId, InventJournalLineInputDto input, CancellationToken ct)
    {
        var result = await _service.AddLineAsync(journalId, input, ct);
        return result.Error == null ? Ok(APIResponse<object>.Ok(result.Journal!)) : UnprocessableEntity(APIResponse<object>.Fail(result.Error));
    }

    [HttpPut("{journalId}/lines/{lineRecId:long}")]
    public async Task<IActionResult> UpdateLine(string journalId, long lineRecId, InventJournalLineInputDto input, CancellationToken ct)
    {
        var result = await _service.UpdateLineAsync(journalId, lineRecId, input, ct);
        return result.Error == null ? Ok(APIResponse<object>.Ok(result.Journal!)) : UnprocessableEntity(APIResponse<object>.Fail(result.Error));
    }

    [HttpDelete("{journalId}/lines/{lineRecId:long}")]
    public async Task<IActionResult> DeleteLine(string journalId, long lineRecId, CancellationToken ct)
    {
        var result = await _service.DeleteLineAsync(journalId, lineRecId, ct);
        return result.Error == null ? Ok(APIResponse<bool>.Ok(result.Deleted)) : Conflict(APIResponse<object>.Fail(result.Error));
    }

    [HttpDelete("{journalId}")]
    public async Task<IActionResult> Delete(string journalId, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(journalId, ct);
        return result.Error == null ? Ok(APIResponse<bool>.Ok(result.Deleted)) : Conflict(APIResponse<object>.Fail(result.Error));
    }


    [HttpPost("{journalId}/post")]
    public async Task<IActionResult> Post(string journalId, CancellationToken ct)
    {
        var result = await _posting.PostAsync(journalId, ct);
        return result.Error == null ? Ok(APIResponse<object>.Ok(result.Result!)) : UnprocessableEntity(APIResponse<object>.Fail(result.Error));
    }
}
