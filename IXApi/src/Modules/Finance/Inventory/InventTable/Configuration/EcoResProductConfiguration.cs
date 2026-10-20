using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory.InventTable;

public sealed class EcoResProductConfiguration : IEntityTypeConfiguration<EcoResProduct>
{
    public void Configure(EntityTypeBuilder<EcoResProduct> builder)
    {
        builder.HasIndex(product => product.DisplayProductNumber).IsUnique();
        builder.Property(product => product.ProductType).HasConversion<int>();
    }
}

public sealed class EcoResProductTranslationConfiguration : IEntityTypeConfiguration<EcoResProductTranslation>
{
    public void Configure(EntityTypeBuilder<EcoResProductTranslation> builder)
    {
        builder.HasIndex(translation => new { translation.Product, translation.LanguageId }).IsUnique();
        builder.HasOne<EcoResProduct>()
            .WithMany()
            .HasForeignKey(translation => translation.Product)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class InventTableConfiguration : IEntityTypeConfiguration<IAX.IXApi.Modules.Finance.Entities.InventTable>
{
    public void Configure(EntityTypeBuilder<IAX.IXApi.Modules.Finance.Entities.InventTable> builder)
    {
        builder.HasIndex(item => new { item.DataAreaId, item.ItemId }).IsUnique();
        builder.HasIndex(item => new { item.DataAreaId, item.Product }).IsUnique();
        builder.HasOne<EcoResProduct>()
            .WithMany()
            .HasForeignKey(item => item.Product)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class InventTableModuleConfiguration : IEntityTypeConfiguration<IAX.IXApi.Modules.Finance.Entities.InventTableModule>
{
    public void Configure(EntityTypeBuilder<IAX.IXApi.Modules.Finance.Entities.InventTableModule> builder)
    {
        builder.HasIndex(module => new { module.DataAreaId, module.ItemId, module.ModuleType }).IsUnique();
    }
}
