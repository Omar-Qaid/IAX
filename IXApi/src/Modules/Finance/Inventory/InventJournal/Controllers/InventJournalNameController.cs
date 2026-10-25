using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventJournalName")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventJournalNameController
    : BaseController<InventJournalName, InventJournalNameDto>
{
    private readonly IInventJournalNameService _journalNameService;

    public InventJournalNameController(
        IInventJournalNameService service,
        ILogger<InventJournalNameController> logger)
        : base(service, logger)
    {
        _journalNameService = service;
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventJournalNameDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventJournalNameDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventJournalNameDto>>> Create(InventJournalNameDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventJournalNameDto>>> Update(string id, InventJournalNameDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(APIResponse<object>.Ok(await _journalNameService.ListAsync(ct)));

    [HttpGet("lookups")]
    public async Task<IActionResult> Lookups(CancellationToken ct) =>
        Ok(APIResponse<object>.Ok(await _journalNameService.LookupsAsync(ct)));

    [HttpPost]
    public async Task<IActionResult> Create(InventJournalNameInputDto input, CancellationToken ct)
    {
        var result = await _journalNameService.CreateAsync(input, ct);
        return result.Error == null
            ? Ok(APIResponse<object>.Ok(result.JournalName!))
            : UnprocessableEntity(APIResponse<object>.Fail(result.Error));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventJournalNameInputDto input, CancellationToken ct)
    {
        var result = await _journalNameService.UpdateAsync(recId, input, ct);
        return result.Error == null
            ? Ok(APIResponse<object>.Ok(result.JournalName!))
            : UnprocessableEntity(APIResponse<object>.Fail(result.Error));
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<APIResponse<bool>>> Delete(long id, CancellationToken cancellationToken = default)
    {
        var result = await _journalNameService.DeleteAsync(id, cancellationToken);
        return result switch
        {
            InventJournalNameDeleteResult.NotFound => NotFound(APIResponse<bool>.Fail("Journal name was not found.")),
            InventJournalNameDeleteResult.InUse => Conflict(APIResponse<bool>.Fail("A journal name referenced by an inventory journal cannot be deleted.")),
            _ => Ok(APIResponse<bool>.Ok(true))
        };
    }
}
