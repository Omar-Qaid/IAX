using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;

[ApiController]
[Route("api/v1/product-statuses")]
[DomainPermission("Inventory", "Transactions")]
public sealed class ProductLifecycleController : BaseController<EcoResProductLifecycleState, ProductLifecycleStateDto>
{
    private readonly IProductLifecycleService _lifecycleService;

    public ProductLifecycleController(
        IProductLifecycleService service,
        ILogger<ProductLifecycleController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<ProductLifecycleController>.Instance)
    {
        _lifecycleService = service;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(APIResponse<object>.Ok(await _lifecycleService.ListAsync(ct)));

    [HttpPost]
    public async Task<IActionResult> Create(ProductLifecycleStateInputDto input, CancellationToken ct)
    {
        var result = await _lifecycleService.SaveAsync(null, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, ProductLifecycleStateInputDto input, CancellationToken ct)
    {
        var result = await _lifecycleService.SaveAsync(recId, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct) =>
        await _lifecycleService.DeleteAsync(recId, ct) is { } error
            ? Conflict(APIResponse<object>.Fail(error)) : Ok(APIResponse<bool>.Ok(true));

    [HttpPut("released-products/{releasedProductRecId:long}")]
    public async Task<IActionResult> Assign(long releasedProductRecId, ReleasedProductStatusInputDto input, CancellationToken ct)
    {
        var result = await _lifecycleService.AssignAsync(releasedProductRecId, input.StateId, ct);
        return result.Item is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Item));
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<ProductLifecycleStateDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<ProductLifecycleStateDto>>>> GetPaged([FromQuery] QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<ProductLifecycleStateDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<ProductLifecycleStateDto>>> Create(ProductLifecycleStateDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<ProductLifecycleStateDto>>> Update(string id, ProductLifecycleStateDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
