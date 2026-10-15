using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory.InventTable;

public sealed class EcoResDistinctProductConfiguration : IEntityTypeConfiguration<EcoResDistinctProduct>
{
    public void Configure(EntityTypeBuilder<EcoResDistinctProduct> builder)
    {
        builder.Property(product => product.RecId).ValueGeneratedNever();
        builder.HasOne<EcoResProduct>()
            .WithOne()
            .HasForeignKey<EcoResDistinctProduct>(product => product.RecId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
