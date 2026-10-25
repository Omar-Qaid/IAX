using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventItemGroupItem")]
[DomainPermission("Inventory", "Transactions")]
public sealed class ItemGroupAssignmentController
    : BaseController<InventItemGroupItem, InventItemGroupItemDto>
{
    private readonly IItemGroupAssignmentService _assignmentService;

    public ItemGroupAssignmentController(
        IItemGroupAssignmentService service,
        ILogger<ItemGroupAssignmentController> logger)
        : base(service, logger)
    {
        _assignmentService = service;
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventItemGroupItemDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventItemGroupItemDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventItemGroupItemDto>>> Create(InventItemGroupItemDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventItemGroupItemDto>>> Update(string id, InventItemGroupItemDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    [HttpPut("{itemId}")]
    public async Task<IActionResult> Assign(string itemId, ItemGroupAssignmentInputDto input, CancellationToken ct)
    {
        var result = await _assignmentService.AssignAsync(itemId, input, ct);
        return result.Assignment is null
            ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<ItemGroupAssignmentRecordDto>.Ok(result.Assignment));
    }
}
