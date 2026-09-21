using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.Structure;

public sealed class OrganizationRoleConfiguration : IEntityTypeConfiguration<OrganizationRole>
{
    public void Configure(EntityTypeBuilder<OrganizationRole> b)
    {
        b.ToTable("OrganizationRoles");
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.HasIndex(x => new { x.DataAreaId, x.Code }).IsUnique();
    }
}

public sealed class OrganizationHierarchyConfiguration : IEntityTypeConfiguration<OrganizationHierarchy>
{
    public void Configure(EntityTypeBuilder<OrganizationHierarchy> b)
    {
        b.ToTable("OrganizationHierarchies");
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Purpose).HasMaxLength(100).IsRequired();
        b.HasIndex(x => new { x.DataAreaId, x.Code }).IsUnique();
    }
}

public sealed class OrganizationHierarchyNodeConfiguration : IEntityTypeConfiguration<OrganizationHierarchyNode>
{
    public void Configure(EntityTypeBuilder<OrganizationHierarchyNode> b)
    {
        b.ToTable("OrganizationHierarchyNodes", t => t.HasCheckConstraint("CK_OrganizationHierarchyNodes_Dates", "[ValidTo] IS NULL OR [ValidTo] > [ValidFrom]"));
        b.HasOne(x => x.Hierarchy).WithMany(x => x.Nodes).HasForeignKey(x => x.HierarchyId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.OrganizationUnit).WithMany(x => x.HierarchyNodes).HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ParentNode).WithMany(x => x.Children).HasForeignKey(x => x.ParentNodeId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.DataAreaId, x.HierarchyId, x.OrganizationUnitId, x.ValidFrom, x.ValidTo });
    }
}

public sealed class HcmPositionConfiguration : IEntityTypeConfiguration<HcmPosition>
{
    public void Configure(EntityTypeBuilder<HcmPosition> b)
    {
        b.ToTable("HcmPositions", t => t.HasCheckConstraint("CK_HcmPositions_Dates", "[ValidTo] IS NULL OR [ValidTo] > [ValidFrom]"));
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.HasIndex(x => new { x.DataAreaId, x.Code }).IsUnique();
        b.HasOne(x => x.OrganizationUnit).WithMany(x => x.Positions).HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Role).WithMany(x => x.Positions).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class HcmReportingHierarchyConfiguration : IEntityTypeConfiguration<HcmReportingHierarchy>
{
    public void Configure(EntityTypeBuilder<HcmReportingHierarchy> b)
    {
        b.ToTable("HcmReportingHierarchies");
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Purpose).HasMaxLength(100).IsRequired();
        b.HasIndex(x => new { x.DataAreaId, x.Code }).IsUnique();
    }
}

public sealed class HcmPositionReportingLineConfiguration : IEntityTypeConfiguration<HcmPositionReportingLine>
{
    public void Configure(EntityTypeBuilder<HcmPositionReportingLine> b)
    {
        b.ToTable("HcmPositionReportingLines", t => t.HasCheckConstraint("CK_HcmPositionReportingLines_Dates", "[ValidTo] IS NULL OR [ValidTo] > [ValidFrom]"));
        b.HasOne(x => x.ReportingHierarchy).WithMany(x => x.ReportingLines).HasForeignKey(x => x.ReportingHierarchyId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.SubordinatePosition).WithMany().HasForeignKey(x => x.SubordinatePositionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ManagerPosition).WithMany().HasForeignKey(x => x.ManagerPositionId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.DataAreaId, x.ReportingHierarchyId, x.ManagerPositionId, x.ValidFrom, x.ValidTo });
        b.HasIndex(x => new { x.DataAreaId, x.ReportingHierarchyId, x.SubordinatePositionId, x.ValidFrom, x.ValidTo });
    }
}
