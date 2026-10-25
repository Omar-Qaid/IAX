using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductCategories;

public sealed class ProductCategoryService : BaseService<EcoResCategoryHierarchy>, IProductCategoryService
{
    public ProductCategoryService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork, currentUser)
    {
    }

    public ProductCategoryService(IUnitOfWork unitOfWork)
        : this(unitOfWork, new NullCurrentUserService())
    {
    }

    private DbContext Db => _unitOfWork.Context;

    public async Task<List<CategoryHierarchyDto>> ListHierarchiesAsync(CancellationToken ct) =>
        await Db.Set<EcoResCategoryHierarchy>().AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new CategoryHierarchyDto(x.RecId, x.Name, x.IsActive)).ToListAsync(ct);

    public async Task<(CategoryHierarchyDto? Row, string? Error)> SaveHierarchyAsync(long? recId, CategoryHierarchyInputDto input, CancellationToken ct)
    {
        var name = input.Name?.Trim() ?? string.Empty;
        if (name.Length == 0 || name.Length > FieldLengths.Name) return (null, "Category hierarchy name is required and must fit its allowed length.");
        var row = recId.HasValue
            ? await Db.Set<EcoResCategoryHierarchy>().FirstOrDefaultAsync(x => x.RecId == recId.Value, ct)
            : new EcoResCategoryHierarchy();
        if (row is null) return (null, "Category hierarchy was not found.");
        if (await Db.Set<EcoResCategoryHierarchy>().AnyAsync(x => x.Name == name && x.RecId != row.RecId, ct))
            return (null, "Category hierarchy name already exists.");
        if (!input.IsActive && recId.HasValue && await Db.Set<EcoResCategory>().AnyAsync(x => x.CategoryHierarchy == row.RecId && x.IsActive, ct))
            return (null, "A hierarchy with active categories cannot be deactivated.");
        if (!recId.HasValue) Db.Add(row);
        row.Name = name;
        row.IsActive = input.IsActive;
        await _unitOfWork.CompleteAsync(ct);
        return (new(row.RecId, row.Name, row.IsActive), null);
    }

    public async Task<string?> DeleteHierarchyAsync(long recId, CancellationToken ct)
    {
        var row = await Db.Set<EcoResCategoryHierarchy>().FirstOrDefaultAsync(x => x.RecId == recId, ct);
        if (row is null) return "Category hierarchy was not found.";
        if (await Db.Set<EcoResCategory>().AnyAsync(x => x.CategoryHierarchy == recId, ct) ||
            await Db.Set<EcoResProductCategory>().AnyAsync(x => x.CategoryHierarchy == recId, ct))
            return "A category hierarchy with categories or product assignments cannot be deleted.";
        Db.Remove(row);
        await _unitOfWork.CompleteAsync(ct);
        return null;
    }

    public async Task<List<ProductCategoryDto>> ListCategoriesAsync(long hierarchyRecId, CancellationToken ct) =>
        await Db.Set<EcoResCategory>().AsNoTracking()
            .Where(x => x.CategoryHierarchy == hierarchyRecId).OrderBy(x => x.Name)
            .Select(x => new ProductCategoryDto(x.RecId, x.CategoryHierarchy, x.ParentCategory, x.Name, x.IsActive)).ToListAsync(ct);

    public async Task<(ProductCategoryDto? Row, string? Error)> SaveCategoryAsync(long? recId, ProductCategoryInputDto input, CancellationToken ct)
    {
        var name = input.Name?.Trim() ?? string.Empty;
        if (name.Length == 0 || name.Length > FieldLengths.Name) return (null, "Category name is required and must fit its allowed length.");
        if (!await Db.Set<EcoResCategoryHierarchy>().AnyAsync(x => x.RecId == input.CategoryHierarchy && x.IsActive, ct))
            return (null, "The category hierarchy does not exist or is inactive.");
        var row = recId.HasValue ? await Db.Set<EcoResCategory>().FirstOrDefaultAsync(x => x.RecId == recId.Value, ct) : new EcoResCategory();
        if (row is null) return (null, "Category was not found.");
        if (recId.HasValue && row.CategoryHierarchy != input.CategoryHierarchy)
            return (null, "Category hierarchy cannot be changed after creation.");
        if (await Db.Set<EcoResCategory>().AnyAsync(x => x.CategoryHierarchy == input.CategoryHierarchy && x.Name == name && x.RecId != row.RecId, ct))
            return (null, "Category name already exists in this hierarchy.");
        if (!input.IsActive && recId.HasValue && await Db.Set<EcoResProductCategory>().AnyAsync(x => x.Category == row.RecId, ct))
            return (null, "A category assigned to products cannot be deactivated.");
        var parentError = await ValidateParentAsync(row.RecId, input.CategoryHierarchy, input.ParentCategory, ct);
        if (parentError is not null) return (null, parentError);
        if (!recId.HasValue) Db.Add(row);
        row.CategoryHierarchy = input.CategoryHierarchy;
        row.ParentCategory = input.ParentCategory;
        row.Name = name;
        row.IsActive = input.IsActive;
        await _unitOfWork.CompleteAsync(ct);
        return (new(row.RecId, row.CategoryHierarchy, row.ParentCategory, row.Name, row.IsActive), null);
    }

    public async Task<string?> DeleteCategoryAsync(long recId, CancellationToken ct)
    {
        var row = await Db.Set<EcoResCategory>().FirstOrDefaultAsync(x => x.RecId == recId && x.CategoryHierarchy != 0, ct);
        if (row is null) return "Category was not found.";
        if (await Db.Set<EcoResCategory>().AnyAsync(x => x.ParentCategory == recId, ct) ||
            await Db.Set<EcoResProductCategory>().AnyAsync(x => x.Category == recId, ct))
            return "A category with children or product assignments cannot be deleted.";
        Db.Remove(row);
        await _unitOfWork.CompleteAsync(ct);
        return null;
    }

    public async Task<(List<ProductCategoryAssignmentDto>? Rows, string? Error)> ListAssignmentsAsync(long productRecId, CancellationToken ct)
    {
        if (!await Db.Set<EcoResProduct>().AnyAsync(x => x.RecId == productRecId, ct))
            return (null, "Shared product was not found.");
        var rows = await (from assignment in Db.Set<EcoResProductCategory>().AsNoTracking()
                          join category in Db.Set<EcoResCategory>().AsNoTracking() on assignment.Category equals category.RecId
                          join hierarchy in Db.Set<EcoResCategoryHierarchy>().AsNoTracking() on assignment.CategoryHierarchy equals hierarchy.RecId
                          where assignment.Product == productRecId
                          orderby assignment.DisplayOrder, category.Name
                          select new ProductCategoryAssignmentDto(assignment.RecId, assignment.Product,
                              assignment.Category, assignment.CategoryHierarchy, category.Name, hierarchy.Name, assignment.DisplayOrder))
            .ToListAsync(ct);
        return (rows, null);
    }

    public async Task<(List<CategoryProductDto>? Rows, string? Error)> ListProductsAsync(long categoryRecId, CancellationToken ct)
    {
        if (!await Db.Set<EcoResCategory>().AnyAsync(x => x.RecId == categoryRecId && x.CategoryHierarchy != 0, ct))
            return (null, "Product category was not found.");
        var rows = await (from assignment in Db.Set<EcoResProductCategory>().AsNoTracking()
                          join product in Db.Set<EcoResProduct>().AsNoTracking() on assignment.Product equals product.RecId
                          where assignment.Category == categoryRecId
                          orderby assignment.DisplayOrder, product.DisplayProductNumber
                          select new CategoryProductDto(product.RecId, product.DisplayProductNumber, assignment.DisplayOrder))
            .ToListAsync(ct);
        return (rows, null);
    }

    public async Task<(ProductCategoryAssignmentDto? Row, string? Error)> SaveAssignmentAsync(
        long productRecId, long? recId, ProductCategoryAssignmentInputDto input, CancellationToken ct)
    {
        if (!await Db.Set<EcoResProduct>().AnyAsync(x => x.RecId == productRecId, ct))
            return (null, "Shared product was not found.");
        var category = await Db.Set<EcoResCategory>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.RecId == input.Category && x.CategoryHierarchy != 0 && x.IsActive, ct);
        if (category is null) return (null, "The selected product category does not exist.");
        var hierarchy = await Db.Set<EcoResCategoryHierarchy>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.RecId == category.CategoryHierarchy && x.IsActive, ct);
        if (hierarchy is null) return (null, "The selected category has no valid hierarchy.");
        var row = recId.HasValue ? await Db.Set<EcoResProductCategory>()
            .FirstOrDefaultAsync(x => x.RecId == recId.Value && x.Product == productRecId, ct) : new EcoResProductCategory();
        if (row is null) return (null, "Product category assignment was not found.");
        if (await Db.Set<EcoResProductCategory>().AnyAsync(x => x.Product == productRecId && x.Category == category.RecId && x.RecId != row.RecId, ct))
            return (null, "The product is already assigned to this category.");
        if (!recId.HasValue) Db.Add(row);
        row.Product = productRecId;
        row.Category = category.RecId;
        row.CategoryHierarchy = hierarchy.RecId;
        row.DisplayOrder = input.DisplayOrder;
        await _unitOfWork.CompleteAsync(ct);
        return (new(row.RecId, row.Product, row.Category, row.CategoryHierarchy,
            category.Name, hierarchy.Name, row.DisplayOrder), null);
    }

    public async Task<string?> DeleteAssignmentAsync(long productRecId, long recId, CancellationToken ct)
    {
        var row = await Db.Set<EcoResProductCategory>()
            .FirstOrDefaultAsync(x => x.RecId == recId && x.Product == productRecId, ct);
        if (row is null) return "Product category assignment was not found.";
        Db.Remove(row);
        await _unitOfWork.CompleteAsync(ct);
        return null;
    }

    private async Task<string?> ValidateParentAsync(long selfId, long hierarchyId, long parentId, CancellationToken ct)
    {
        if (parentId == 0) return null;
        var ancestors = await Db.Set<EcoResCategory>().AsNoTracking()
            .Where(x => x.CategoryHierarchy == hierarchyId).Select(x => new { x.RecId, x.ParentCategory }).ToListAsync(ct);
        var visited = new HashSet<long>();
        var current = parentId;
        while (current != 0)
        {
            if (current == selfId || !visited.Add(current)) return "Category parent would create a cycle.";
            var parent = ancestors.FirstOrDefault(x => x.RecId == current);
            if (parent is null) return "Parent category must belong to the selected hierarchy.";
            current = parent.ParentCategory;
        }
        return null;
    }

    private sealed class NullCurrentUserService : ICurrentUserService
    {
        public string GetCurrentUserId() => string.Empty;
        public string GetOwnerAccountId() => string.Empty;
        public string GetDataAreaId() => string.Empty;
    }
}
