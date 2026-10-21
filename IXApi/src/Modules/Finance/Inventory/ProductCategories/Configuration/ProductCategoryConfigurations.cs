using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Configuration;

public sealed class EcoResCategoryHierarchyConfiguration : IEntityTypeConfiguration<EcoResCategoryHierarchy>
{
    public void Configure(EntityTypeBuilder<EcoResCategoryHierarchy> builder) =>
        builder.HasIndex(x => x.Name).IsUnique();
}

public sealed class EcoResProductCategoryConfiguration : IEntityTypeConfiguration<EcoResProductCategory>
{
    public void Configure(EntityTypeBuilder<EcoResProductCategory> builder)
    {
        builder.HasIndex(x => new { x.Product, x.Category }).IsUnique();
        builder.HasIndex(x => x.CategoryHierarchy);
        builder.HasOne<EcoResProduct>().WithMany().HasForeignKey(x => x.Product).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EcoResCategory>().WithMany().HasForeignKey(x => x.Category).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EcoResCategoryHierarchy>().WithMany().HasForeignKey(x => x.CategoryHierarchy).OnDelete(DeleteBehavior.Restrict);
    }
}
