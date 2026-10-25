using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductCategories;

public interface IProductCategoryService : IBaseService<EcoResCategoryHierarchy>
{
    Task<List<CategoryHierarchyDto>> ListHierarchiesAsync(CancellationToken ct);
    Task<(CategoryHierarchyDto? Row, string? Error)> SaveHierarchyAsync(long? recId, CategoryHierarchyInputDto input, CancellationToken ct);
    Task<string?> DeleteHierarchyAsync(long recId, CancellationToken ct);
    Task<List<ProductCategoryDto>> ListCategoriesAsync(long hierarchyRecId, CancellationToken ct);
    Task<(ProductCategoryDto? Row, string? Error)> SaveCategoryAsync(long? recId, ProductCategoryInputDto input, CancellationToken ct);
    Task<string?> DeleteCategoryAsync(long recId, CancellationToken ct);
    Task<(List<ProductCategoryAssignmentDto>? Rows, string? Error)> ListAssignmentsAsync(long productRecId, CancellationToken ct);
    Task<(List<CategoryProductDto>? Rows, string? Error)> ListProductsAsync(long categoryRecId, CancellationToken ct);
    Task<(ProductCategoryAssignmentDto? Row, string? Error)> SaveAssignmentAsync(long productRecId, long? recId, ProductCategoryAssignmentInputDto input, CancellationToken ct);
    Task<string?> DeleteAssignmentAsync(long productRecId, long recId, CancellationToken ct);
}
