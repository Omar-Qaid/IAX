using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventTable/released-products")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class ReleasedProductListController(IReleasedProductListService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken = default)
    {
        var products = await service.ListAsync(cancellationToken);
        if (products is null)
            return BadRequest(APIResponse<List<ReleasedProductListDto>>.Fail(
                "A company must be selected to view released products."));

        return Ok(APIResponse<List<ReleasedProductListDto>>.Ok(products));
    }

    [HttpGet("lookups")]
    public async Task<IActionResult> Lookups(CancellationToken cancellationToken)
    {
        var result = await service.GetLookupsAsync(cancellationToken);
        return result is null ? BadRequest(APIResponse<ReleasedProductLookupDto>.Fail("A company must be selected.")) : Ok(APIResponse<ReleasedProductLookupDto>.Ok(result));
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateReleased(CreateReleasedProductDto input, CancellationToken cancellationToken)
    {
        var result = await service.CreateReleasedAsync(input, cancellationToken);
        return result.Product is null ? BadRequest(APIResponse<ReleasedProductListDto>.Fail(result.Error!)) : Ok(APIResponse<ReleasedProductListDto>.Ok(result.Product));
    }

    [HttpPost("shared-products")]
    public async Task<IActionResult> CreateShared(CreateSharedProductDto input, CancellationToken cancellationToken)
    {
        var result = await service.CreateSharedAsync(input, cancellationToken);
        return result.Product is null
            ? Conflict(APIResponse<ReleasedProductListDto>.Fail(result.Error ?? "The product could not be created."))
            : Ok(APIResponse<ReleasedProductListDto>.Ok(result.Product));
    }

    [HttpPost("release")]
    public async Task<IActionResult> Release(ReleaseProductDto input, CancellationToken cancellationToken)
    {
        var result = await service.ReleaseAsync(input, cancellationToken);
        return result.Product is null
            ? Conflict(APIResponse<ReleasedProductListDto>.Fail(result.Error ?? "The product could not be released."))
            : Ok(APIResponse<ReleasedProductListDto>.Ok(result.Product));
    }

    [HttpPut("{recId:long}/configuration")]
    public async Task<IActionResult> Configure(long recId, ConfigureReleasedProductDto input, CancellationToken cancellationToken)
    {
        var result = await service.ConfigureAsync(recId, input, cancellationToken);
        return result.Product is null
            ? BadRequest(APIResponse<ReleasedProductListDto>.Fail(result.Error ?? "The product could not be configured."))
            : Ok(APIResponse<ReleasedProductListDto>.Ok(result.Product));
    }

    [HttpPut("{recId:long}/groups")]
    public async Task<IActionResult> AssignGroups(long recId, ReleasedProductGroupAssignmentDto input, CancellationToken cancellationToken)
    {
        var result = await service.AssignGroupsAsync(recId, input, cancellationToken);
        return result.Product is null
            ? BadRequest(APIResponse<ReleasedProductListDto>.Fail(result.Error ?? "The product groups could not be assigned."))
            : Ok(APIResponse<ReleasedProductListDto>.Ok(result.Product));
    }

    [HttpPut("{recId:long}/units")]
    public async Task<IActionResult> AssignUnits(long recId, ReleasedProductUnitAssignmentDto input, CancellationToken cancellationToken)
    {
        var result = await service.AssignUnitsAsync(recId, input, cancellationToken);
        return result.Product is null
            ? BadRequest(APIResponse<ReleasedProductListDto>.Fail(result.Error ?? "The product units could not be assigned."))
            : Ok(APIResponse<ReleasedProductListDto>.Ok(result.Product));
    }

    [HttpGet("{recId:long}/validate")]
    public async Task<IActionResult> Validate(long recId, CancellationToken cancellationToken)
    {
        var result = await service.ValidateAsync(recId, cancellationToken);
        return result is null
            ? NotFound(APIResponse<ReleasedProductValidationDto>.Fail("The released product was not found."))
            : Ok(APIResponse<ReleasedProductValidationDto>.Ok(result));
    }
}
