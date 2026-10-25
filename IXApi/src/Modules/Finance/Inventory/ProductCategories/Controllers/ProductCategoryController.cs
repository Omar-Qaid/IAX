using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductCategories;

[ApiController]
[Route("api/v1/product-categories")]
[DomainPermission("Inventory", "Transactions")]
public sealed class ProductCategoryController : BaseController<EcoResCategoryHierarchy, CategoryHierarchyDto>
{
    private readonly IProductCategoryService _categoryService;

    public ProductCategoryController(
        IProductCategoryService service,
        ILogger<ProductCategoryController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<ProductCategoryController>.Instance)
    {
        _categoryService = service;
    }

    [HttpGet("hierarchies")]
    public async Task<IActionResult> Hierarchies(CancellationToken ct) => Ok(APIResponse<object>.Ok(await _categoryService.ListHierarchiesAsync(ct)));

    [HttpPost("hierarchies")]
    public async Task<IActionResult> CreateHierarchy(CategoryHierarchyInputDto input, CancellationToken ct)
    {
        var result = await _categoryService.SaveHierarchyAsync(null, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpPut("hierarchies/{recId:long}")]
    public async Task<IActionResult> UpdateHierarchy(long recId, CategoryHierarchyInputDto input, CancellationToken ct)
    {
        var result = await _categoryService.SaveHierarchyAsync(recId, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpDelete("hierarchies/{recId:long}")]
    public async Task<IActionResult> DeleteHierarchy(long recId, CancellationToken ct) =>
        await _categoryService.DeleteHierarchyAsync(recId, ct) is { } error
            ? Conflict(APIResponse<object>.Fail(error)) : Ok(APIResponse<bool>.Ok(true));

    [HttpGet("hierarchies/{hierarchyRecId:long}/categories")]
    public async Task<IActionResult> Categories(long hierarchyRecId, CancellationToken ct) =>
        Ok(APIResponse<object>.Ok(await _categoryService.ListCategoriesAsync(hierarchyRecId, ct)));

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(ProductCategoryInputDto input, CancellationToken ct)
    {
        var result = await _categoryService.SaveCategoryAsync(null, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpGet("categories/{categoryRecId:long}/products")]
    public async Task<IActionResult> Products(long categoryRecId, CancellationToken ct)
    {
        var result = await _categoryService.ListProductsAsync(categoryRecId, ct);
        return result.Rows is null ? NotFound(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Rows));
    }

    [HttpPut("categories/{recId:long}")]
    public async Task<IActionResult> UpdateCategory(long recId, ProductCategoryInputDto input, CancellationToken ct)
    {
        var result = await _categoryService.SaveCategoryAsync(recId, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpDelete("categories/{recId:long}")]
    public async Task<IActionResult> DeleteCategory(long recId, CancellationToken ct) =>
        await _categoryService.DeleteCategoryAsync(recId, ct) is { } error
            ? Conflict(APIResponse<object>.Fail(error)) : Ok(APIResponse<bool>.Ok(true));

    [HttpGet("products/{productRecId:long}")]
    public async Task<IActionResult> Assignments(long productRecId, CancellationToken ct)
    {
        var result = await _categoryService.ListAssignmentsAsync(productRecId, ct);
        return result.Rows is null ? NotFound(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Rows));
    }

    [HttpPost("products/{productRecId:long}")]
    public async Task<IActionResult> Assign(long productRecId, ProductCategoryAssignmentInputDto input, CancellationToken ct)
    {
        var result = await _categoryService.SaveAssignmentAsync(productRecId, null, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpPut("products/{productRecId:long}/{recId:long}")]
    public async Task<IActionResult> UpdateAssignment(long productRecId, long recId, ProductCategoryAssignmentInputDto input, CancellationToken ct)
    {
        var result = await _categoryService.SaveAssignmentAsync(productRecId, recId, input, ct);
        return result.Row is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<object>.Ok(result.Row));
    }

    [HttpDelete("products/{productRecId:long}/{recId:long}")]
    public async Task<IActionResult> DeleteAssignment(long productRecId, long recId, CancellationToken ct) =>
        await _categoryService.DeleteAssignmentAsync(productRecId, recId, ct) is { } error
            ? NotFound(APIResponse<object>.Fail(error)) : Ok(APIResponse<bool>.Ok(true));

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<CategoryHierarchyDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<CategoryHierarchyDto>>>> GetPaged([FromQuery] QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<CategoryHierarchyDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<CategoryHierarchyDto>>> Create(CategoryHierarchyDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<CategoryHierarchyDto>>> Update(string id, CategoryHierarchyDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
