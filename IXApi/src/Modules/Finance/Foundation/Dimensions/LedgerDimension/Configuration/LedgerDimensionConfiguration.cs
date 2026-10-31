using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension.Configuration
{
    /// <summary>
    /// An account structure (StructureType AccountStructure) or other dimension hierarchy. The
    /// entity existed with no configuration, so the table was never created.
    /// </summary>
    public class DimensionHierarchyConfiguration : IEntityTypeConfiguration<DimensionHierarchy>
    {
        public void Configure(EntityTypeBuilder<DimensionHierarchy> builder)
        {
            builder.ToTable("DimensionHierarchy");
            builder.HasIndex(x => new { x.DataAreaId, x.Name }).IsUnique();
        }
    }

    /// <summary>
    /// The ordered dimension values behind one ledger account combination. A group is shared by
    /// every combination that resolves to the same values, which is why the link is its own table.
    /// </summary>
    public class DimensionAttributeValueGroupConfiguration : IEntityTypeConfiguration<DimensionAttributeValueGroup>
    {
        public void Configure(EntityTypeBuilder<DimensionAttributeValueGroup> builder)
        {
            builder.ToTable("DimensionAttributeValueGroup");
            builder.HasIndex(x => new { x.DataAreaId, x.DimensionHierarchy });
            builder.HasOne<DimensionHierarchy>().WithMany()
                .HasForeignKey(x => x.DimensionHierarchy).OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>One value of a group, at its level in the structure.</summary>
    public class DimensionAttributeLevelValueConfiguration : IEntityTypeConfiguration<DimensionAttributeLevelValue>
    {
        public void Configure(EntityTypeBuilder<DimensionAttributeLevelValue> builder)
        {
            builder.ToTable("DimensionAttributeLevelValue");
            builder.HasIndex(x => new { x.DataAreaId, x.DimensionAttributeValueGroup, x.Ordinal });
            builder.HasOne<DimensionAttributeValueGroup>().WithMany()
                .HasForeignKey(x => x.DimensionAttributeValueGroup).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<DimensionAttributeValue>().WithMany()
                .HasForeignKey(x => x.DimensionAttributeValue).OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>Links a ledger account combination to the value group that supplies its dimensions.</summary>
    public class DimensionAttributeValueGroupCombinationConfiguration
        : IEntityTypeConfiguration<DimensionAttributeValueGroupCombination>
    {
        public void Configure(EntityTypeBuilder<DimensionAttributeValueGroupCombination> builder)
        {
            builder.ToTable("DimensionAttributeValueGroupCombination");
            builder.HasIndex(x => new { x.DataAreaId, x.DimensionAttributeValueCombination });
            builder.HasOne<DimensionAttributeValueCombination>().WithMany()
                .HasForeignKey(x => x.DimensionAttributeValueCombination).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<DimensionAttributeValueGroup>().WithMany()
                .HasForeignKey(x => x.DimensionAttributeValueGroup).OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>
    /// The combination was already in the EF model, reached only through other entities' foreign
    /// keys, and carried no index at all. Resolution looks a combination up by its display value,
    /// so that lookup is made unique here.
    /// </summary>
    public class DimensionAttributeValueCombinationConfiguration
        : IEntityTypeConfiguration<DimensionAttributeValueCombination>
    {
        public void Configure(EntityTypeBuilder<DimensionAttributeValueCombination> builder)
        {
            builder.ToTable("DimensionAttributeValueCombination");
            builder.HasIndex(x => new { x.DataAreaId, x.DisplayValue }).IsUnique();
        }
    }
}
