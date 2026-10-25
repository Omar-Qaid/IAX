using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using InventPostingEntity = IAX.IXApi.Modules.Finance.Entities.InventPosting;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventPosting")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventPostingController : BaseController<InventPostingEntity, InventPostingRuleDto>
{
    private readonly IInventPostingService _postingService;

    public InventPostingController(
        IInventPostingService service,
        ILogger<InventPostingController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<InventPostingController>.Instance)
    {
        _postingService = service;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int accountType, CancellationToken ct)
    {
        var rows = await _postingService.ListAsync(accountType, ct);
        return rows is null
            ? BadRequest(APIResponse<List<InventPostingRuleDto>>.Fail("A legal entity must be selected."))
            : Ok(APIResponse<List<InventPostingRuleDto>>.Ok(rows));
    }

    [HttpGet("lookups")]
    public async Task<IActionResult> Lookups(CancellationToken ct)
    {
        var rows = await _postingService.LookupsAsync(ct);
        return rows is null
            ? BadRequest(APIResponse<InventPostingLookupsDto>.Fail("A legal entity must be selected."))
            : Ok(APIResponse<InventPostingLookupsDto>.Ok(rows));
    }

    [HttpPost]
    public override async Task<ActionResult<APIResponse<InventPostingRuleDto>>> Create(InventPostingRuleDto input, CancellationToken cancellationToken = default)
    {
        var result = await _postingService.CreateAsync(input, cancellationToken);
        return result.Rule is null
            ? BadRequest(APIResponse<InventPostingRuleDto>.Fail(result.Error!))
            : Ok(APIResponse<InventPostingRuleDto>.Ok(result.Rule));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventPostingRuleDto input, CancellationToken ct)
    {
        var result = await _postingService.UpdateAsync(recId, input, ct);
        return result.Rule is null
            ? BadRequest(APIResponse<InventPostingRuleDto>.Fail(result.Error!))
            : Ok(APIResponse<InventPostingRuleDto>.Ok(result.Rule));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var result = await _postingService.DeleteAsync(recId, ct);
        return result.Deleted
            ? Ok(APIResponse<bool>.Ok(true))
            : NotFound(APIResponse<bool>.Fail(result.Error!));
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventPostingRuleDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventPostingRuleDto>>>> GetPaged([FromQuery] QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventPostingRuleDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventPostingRuleDto>>> Update(string id, InventPostingRuleDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
