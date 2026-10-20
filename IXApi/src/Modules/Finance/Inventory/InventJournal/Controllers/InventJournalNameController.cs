using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventJournalName")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventJournalNameController : ControllerBase
{
    private readonly IInventJournalNameService _service;
    public InventJournalNameController(IInventJournalNameService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(APIResponse<object>.Ok(await _service.ListAsync(ct)));

    [HttpGet("lookups")]
    public async Task<IActionResult> Lookups(CancellationToken ct) => Ok(APIResponse<object>.Ok(await _service.LookupsAsync(ct)));

    [HttpPost]
    public async Task<IActionResult> Create(InventJournalNameInputDto input, CancellationToken ct)
    {
        var result = await _service.CreateAsync(input, ct);
        return result.Error == null ? Ok(APIResponse<object>.Ok(result.JournalName!)) : UnprocessableEntity(APIResponse<object>.Fail(result.Error));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventJournalNameInputDto input, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(recId, input, ct);
        return result.Error == null ? Ok(APIResponse<object>.Ok(result.JournalName!)) : UnprocessableEntity(APIResponse<object>.Fail(result.Error));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(recId, ct);
        return result switch
        {
            InventJournalNameDeleteResult.NotFound => NotFound(APIResponse<object>.Fail("Journal name was not found.")),
            InventJournalNameDeleteResult.InUse => Conflict(APIResponse<object>.Fail("A journal name referenced by an inventory journal cannot be deleted.")),
            _ => Ok(APIResponse<bool>.Ok(true))
        };
    }
}
