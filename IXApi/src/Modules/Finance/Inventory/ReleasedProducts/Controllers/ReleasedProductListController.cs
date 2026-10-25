using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventTable/released-products")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class ReleasedProductListController
    : BaseController<ReleasedItem, ReleasedProductListDto>
{
    private readonly IReleasedProductListService _releasedProductService;

    public ReleasedProductListController(
        IReleasedProductListService service,
        ILogger<ReleasedProductListController> logger)
        : base(service, logger)
    {
        _releasedProductService = service;
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<ReleasedProductListDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<ReleasedProductListDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<ReleasedProductListDto>>> Create(ReleasedProductListDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<ReleasedProductListDto>>> Update(string id, ReleasedProductListDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken = default)
    {
        var products = await _releasedProductService.ListAsync(cancellationToken);
        if (products is null)
            return BadRequest(APIResponse<List<ReleasedProductListDto>>.Fail(
                "A company must be selected to view released products."));

        return Ok(APIResponse<List<ReleasedProductListDto>>.Ok(products));
    }

    [HttpGet("lookups")]
    public async Task<IActionResult> Lookups(CancellationToken cancellationToken)
    {
        var result = await _releasedProductService.GetLookupsAsync(cancellationToken);
        return result is null ? BadRequest(APIResponse<ReleasedProductLookupDto>.Fail("A company must be selected.")) : Ok(APIResponse<ReleasedProductLookupDto>.Ok(result));
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateReleased(CreateReleasedProductDto input, CancellationToken cancellationToken)
    {
        var result = await _releasedProductService.CreateReleasedAsync(input, cancellationToken);
        return result.Product is null ? BadRequest(APIResponse<ReleasedProductListDto>.Fail(result.Error!)) : Ok(APIResponse<ReleasedProductListDto>.Ok(result.Product));
    }

    [HttpPost("shared-products")]
    public async Task<IActionResult> CreateShared(CreateSharedProductDto input, CancellationToken cancellationToken)
    {
        var result = await _releasedProductService.CreateSharedAsync(input, cancellationToken);
        return result.Product is null
            ? Conflict(APIResponse<ReleasedProductListDto>.Fail(result.Error ?? "The product could not be created."))
            : Ok(APIResponse<ReleasedProductListDto>.Ok(result.Product));
    }

    [HttpPost("release")]
    public async Task<IActionResult> Release(ReleaseProductDto input, CancellationToken cancellationToken)
    {
        var result = await _releasedProductService.ReleaseAsync(input, cancellationToken);
        return result.Product is null
            ? Conflict(APIResponse<ReleasedProductListDto>.Fail(result.Error ?? "The product could not be released."))
            : Ok(APIResponse<ReleasedProductListDto>.Ok(result.Product));
    }

    [HttpPut("{recId:long}/configuration")]
    public async Task<IActionResult> Configure(long recId, ConfigureReleasedProductDto input, CancellationToken cancellationToken)
    {
        var result = await _releasedProductService.ConfigureAsync(recId, input, cancellationToken);
        return result.Product is null
            ? BadRequest(APIResponse<ReleasedProductListDto>.Fail(result.Error ?? "The product could not be configured."))
            : Ok(APIResponse<ReleasedProductListDto>.Ok(result.Product));
    }

    [HttpPut("{recId:long}/groups")]
    public async Task<IActionResult> AssignGroups(long recId, ReleasedProductGroupAssignmentDto input, CancellationToken cancellationToken)
    {
        var result = await _releasedProductService.AssignGroupsAsync(recId, input, cancellationToken);
        return result.Product is null
            ? BadRequest(APIResponse<ReleasedProductListDto>.Fail(result.Error ?? "The product groups could not be assigned."))
            : Ok(APIResponse<ReleasedProductListDto>.Ok(result.Product));
    }

    [HttpPut("{recId:long}/units")]
    public async Task<IActionResult> AssignUnits(long recId, ReleasedProductUnitAssignmentDto input, CancellationToken cancellationToken)
    {
        var result = await _releasedProductService.AssignUnitsAsync(recId, input, cancellationToken);
        return result.Product is null
            ? BadRequest(APIResponse<ReleasedProductListDto>.Fail(result.Error ?? "The product units could not be assigned."))
            : Ok(APIResponse<ReleasedProductListDto>.Ok(result.Product));
    }

    [HttpGet("{recId:long}/validate")]
    public async Task<IActionResult> Validate(long recId, CancellationToken cancellationToken)
    {
        var result = await _releasedProductService.ValidateAsync(recId, cancellationToken);
        return result is null
            ? NotFound(APIResponse<ReleasedProductValidationDto>.Fail("The released product was not found."))
            : Ok(APIResponse<ReleasedProductValidationDto>.Ok(result));
    }

    [HttpGet("{recId:long}/readiness")]
    public async Task<IActionResult> Readiness(long recId, CancellationToken cancellationToken)
    {
        var result = await _releasedProductService.GetReadinessAsync(recId, cancellationToken);
        return result is null
            ? NotFound(APIResponse<object>.Fail("The released product was not found in the selected company."))
            : Ok(APIResponse<ReleasedItemReadinessDto>.Ok(result));
    }
}
