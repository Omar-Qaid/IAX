using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductCategories;

[ApiController]
[Route("api/v1/product-categories")]
[DomainPermission("Inventory", "Transactions")]
public sealed class ProductCategoryController(IProductCategoryService service) : ControllerBase
{
    [HttpGet("hierarchies")]
    public async Task<IActionResult> Hierarchies(CancellationToken ct) => Ok(APIResponse<object>.Ok(await service.ListHierarchiesAsync(ct)));

    [HttpPost("hierarchies")]
    public async Task<IActionResult> CreateHierarchy(CategoryHierarchyInputDto input, CancellationToken ct)
    {
        var result = await service.SaveHierarchyAsync(null, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpPut("hierarchies/{recId:long}")]
    public async Task<IActionResult> UpdateHierarchy(long recId, CategoryHierarchyInputDto input, CancellationToken ct)
    {
        var result = await service.SaveHierarchyAsync(recId, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpDelete("hierarchies/{recId:long}")]
    public async Task<IActionResult> DeleteHierarchy(long recId, CancellationToken ct) =>
        await service.DeleteHierarchyAsync(recId, ct) is { } error
            ? Conflict(APIResponse<object>.Fail(error)) : Ok(APIResponse<bool>.Ok(true));

    [HttpGet("hierarchies/{hierarchyRecId:long}/categories")]
    public async Task<IActionResult> Categories(long hierarchyRecId, CancellationToken ct) =>
        Ok(APIResponse<object>.Ok(await service.ListCategoriesAsync(hierarchyRecId, ct)));

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(ProductCategoryInputDto input, CancellationToken ct)
    {
        var result = await service.SaveCategoryAsync(null, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpPut("categories/{recId:long}")]
    public async Task<IActionResult> UpdateCategory(long recId, ProductCategoryInputDto input, CancellationToken ct)
    {
        var result = await service.SaveCategoryAsync(recId, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpDelete("categories/{recId:long}")]
    public async Task<IActionResult> DeleteCategory(long recId, CancellationToken ct) =>
        await service.DeleteCategoryAsync(recId, ct) is { } error
            ? Conflict(APIResponse<object>.Fail(error)) : Ok(APIResponse<bool>.Ok(true));

    [HttpGet("products/{productRecId:long}")]
    public async Task<IActionResult> Assignments(long productRecId, CancellationToken ct)
    {
        var result = await service.ListAssignmentsAsync(productRecId, ct);
        return result.Rows is null ? NotFound(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Rows));
    }

    [HttpPost("products/{productRecId:long}")]
    public async Task<IActionResult> Assign(long productRecId, ProductCategoryAssignmentInputDto input, CancellationToken ct)
    {
        var result = await service.SaveAssignmentAsync(productRecId, null, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpPut("products/{productRecId:long}/{recId:long}")]
    public async Task<IActionResult> UpdateAssignment(long productRecId, long recId, ProductCategoryAssignmentInputDto input, CancellationToken ct)
    {
        var result = await service.SaveAssignmentAsync(productRecId, recId, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpDelete("products/{productRecId:long}/{recId:long}")]
    public async Task<IActionResult> DeleteAssignment(long productRecId, long recId, CancellationToken ct) =>
        await service.DeleteAssignmentAsync(productRecId, recId, ct) is { } error
            ? NotFound(APIResponse<object>.Fail(error)) : Ok(APIResponse<bool>.Ok(true));
}
