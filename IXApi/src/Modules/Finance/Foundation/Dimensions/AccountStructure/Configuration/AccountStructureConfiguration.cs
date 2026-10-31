using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure.Configuration
{
    public class DimensionHierarchyLevelConfiguration : IEntityTypeConfiguration<DimensionHierarchyLevel>
    {
        public void Configure(EntityTypeBuilder<DimensionHierarchyLevel> builder)
        {
            builder.ToTable("DimensionHierarchyLevel");
            // A structure has one segment per position, and uses a dimension at most once.
            builder.HasIndex(x => new { x.DataAreaId, x.DimensionHierarchy, x.Level }).IsUnique();
            builder.HasIndex(x => new { x.DataAreaId, x.DimensionHierarchy, x.DimensionAttribute }).IsUnique();
            builder.HasOne<DimensionHierarchy>().WithMany()
                .HasForeignKey(x => x.DimensionHierarchy).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<DimensionAttribute>().WithMany()
                .HasForeignKey(x => x.DimensionAttribute).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class DimensionConstraintTreeConfiguration : IEntityTypeConfiguration<DimensionConstraintTree>
    {
        public void Configure(EntityTypeBuilder<DimensionConstraintTree> builder)
        {
            builder.ToTable("DimensionConstraintTree");
            // One activated tree per structure.
            builder.HasIndex(x => new { x.DataAreaId, x.DimensionHierarchy }).IsUnique();
            builder.HasOne<DimensionHierarchy>().WithMany()
                .HasForeignKey(x => x.DimensionHierarchy).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class DimensionConstraintNodeConfiguration : IEntityTypeConfiguration<DimensionConstraintNode>
    {
        public void Configure(EntityTypeBuilder<DimensionConstraintNode> builder)
        {
            builder.ToTable("DimensionConstraintNode");
            builder.HasIndex(x => new { x.DataAreaId, x.DimensionConstraintTree, x.ParentConstraintNode, x.Ordinal });
            builder.HasOne<DimensionConstraintTree>().WithMany()
                .HasForeignKey(x => x.DimensionConstraintTree).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<DimensionHierarchyLevel>().WithMany()
                .HasForeignKey(x => x.DimensionHierarchyLevel).OnDelete(DeleteBehavior.Restrict);
            // ParentConstraintNode is a self reference that is 0 at the root, so it carries no
            // foreign key: a required one would reject every root node.
        }
    }

    public class DimensionConstraintNodeCriteriaConfiguration
        : IEntityTypeConfiguration<DimensionConstraintNodeCriteria>
    {
        public void Configure(EntityTypeBuilder<DimensionConstraintNodeCriteria> builder)
        {
            builder.ToTable("DimensionConstraintNodeCriteria");
            builder.HasIndex(x => new { x.DataAreaId, x.DimensionConstraintNode, x.Ordinal }).IsUnique();
            builder.HasOne<DimensionConstraintNode>().WithMany()
                .HasForeignKey(x => x.DimensionConstraintNode).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
