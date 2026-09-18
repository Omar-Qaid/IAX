using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;

public class HcmWorkerOrganizationAssignmentConfiguration : IEntityTypeConfiguration<HcmWorkerOrganizationAssignment>
{
    public void Configure(EntityTypeBuilder<HcmWorkerOrganizationAssignment> builder)
    {
        builder.ToTable("HcmWorkerOrganizationAssignments", t => t.HasCheckConstraint("CK_HcmWorkerOrganizationAssignments_Dates", "[ValidTo] IS NULL OR [ValidTo] > [ValidFrom]"));
        builder.Property(x => x.DataAreaId).HasMaxLength(4).IsRequired();
        builder.HasOne(x => x.Position).WithMany(x => x.WorkerAssignments).HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.OrganizationRole).WithMany(x => x.WorkerAssignments).HasForeignKey(x => x.OrganizationRoleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.DataAreaId, x.HcmWorkerId, x.IsPrimary, x.ValidFrom, x.ValidTo });
        builder.HasIndex(x => new { x.DataAreaId, x.PositionId, x.ValidFrom, x.ValidTo });
        builder.HasIndex(x => new { x.DataAreaId, x.OrganizationRoleId, x.ValidFrom, x.ValidTo });
        builder.HasKey(x => x.RecId);
        builder.Property(x => x.RecId).ValueGeneratedOnAdd();
        builder.Property(x => x.AssignmentRole).HasColumnType("tinyint");
        builder.Property(x => x.ValidFrom).HasColumnType("date");
        builder.Property(x => x.ValidTo).HasColumnType("date");
        builder.Property(x => x.IsPrimary).HasDefaultValue(true);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasOne(x => x.HcmWorker).WithMany(x => x.WorkerOrganizationAssignments)
            .HasForeignKey(x => x.HcmWorkerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.OrganizationUnit).WithMany(x => x.WorkerOrganizationAssignments)
            .HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.HcmWorkerId, x.ValidFrom, x.ValidTo });
        builder.HasIndex(x => new { x.OrganizationUnitId, x.AssignmentRole, x.ValidFrom, x.ValidTo });
    }
}
