namespace IAX.IXApi.Modules.Finance.Inventory.ProductCategories;

public sealed record CategoryHierarchyDto(long RecId, string Name);
public sealed record CategoryHierarchyInputDto(string Name);
public sealed record ProductCategoryDto(long RecId, long CategoryHierarchy, long ParentCategory, string Name);
public sealed record ProductCategoryInputDto(long CategoryHierarchy, long ParentCategory, string Name);
public sealed record ProductCategoryAssignmentDto(long RecId, long Product, long Category,
    long CategoryHierarchy, string CategoryName, string HierarchyName, int DisplayOrder);
public sealed record ProductCategoryAssignmentInputDto(long Category, int DisplayOrder = 0);
