namespace IAX.IXApi.Modules.Finance.Inventory.ProductCategories;

public sealed record CategoryHierarchyDto(long RecId, string Name, bool IsActive);
public sealed record CategoryHierarchyInputDto(string Name, bool IsActive = true);
public sealed record ProductCategoryDto(long RecId, long CategoryHierarchy, long ParentCategory, string Name, bool IsActive);
public sealed record ProductCategoryInputDto(long CategoryHierarchy, long ParentCategory, string Name, bool IsActive = true);
public sealed record CategoryProductDto(long ProductRecId, string ProductNumber, int DisplayOrder);
public sealed record ProductCategoryAssignmentDto(long RecId, long Product, long Category,
    long CategoryHierarchy, string CategoryName, string HierarchyName, int DisplayOrder);
public sealed record ProductCategoryAssignmentInputDto(long Category, int DisplayOrder = 0);
