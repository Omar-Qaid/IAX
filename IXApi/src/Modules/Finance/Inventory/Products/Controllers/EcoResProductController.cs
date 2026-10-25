using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/EcoResProduct")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class EcoResProductController : BaseController<EcoResProduct, EcoResProductDto>
{
    private readonly IEcoResProductService _productService;

    public EcoResProductController(
        IEcoResProductService service,
        ILogger<EcoResProductController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<EcoResProductController>.Instance)
    {
        _productService = service;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(APIResponse<List<EcoResProductDto>>.Ok(await _productService.ListAsync(ct)));

    [HttpPost]
    public async Task<IActionResult> Create(EcoResProductInputDto input, CancellationToken ct)
    {
        var result = await _productService.CreateAsync(input, ct);
        return result.Product is null ? Conflict(APIResponse<EcoResProductDto>.Fail(result.Error!)) : Ok(APIResponse<EcoResProductDto>.Ok(result.Product));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, EcoResProductInputDto input, CancellationToken ct)
    {
        var result = await _productService.UpdateAsync(recId, input, ct);
        return result.Product is null ? BadRequest(APIResponse<EcoResProductDto>.Fail(result.Error!)) : Ok(APIResponse<EcoResProductDto>.Ok(result.Product));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var result = await _productService.DeleteAsync(recId, ct);
        return result.Deleted ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<bool>.Fail(result.Error!));
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<EcoResProductDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<EcoResProductDto>>>> GetPaged([FromQuery] QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<EcoResProductDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<EcoResProductDto>>> Create(EcoResProductDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<EcoResProductDto>>> Update(string id, EcoResProductDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
