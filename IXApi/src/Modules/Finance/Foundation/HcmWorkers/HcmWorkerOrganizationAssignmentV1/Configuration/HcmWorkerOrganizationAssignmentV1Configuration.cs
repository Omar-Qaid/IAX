using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;

public class HcmWorkerOrganizationAssignmentV1Configuration : IEntityTypeConfiguration<HcmWorkerOrganizationAssignmentV1>
{
    public void Configure(EntityTypeBuilder<HcmWorkerOrganizationAssignmentV1> builder)
    {
        builder.ToTable("HcmWorkerOrganizationAssignmentsV1", t => t.HasCheckConstraint(
            "CK_HcmWorkerOrganizationAssignmentsV1_Dates",
            "[ValidTo] IS NULL OR [ValidTo] > [ValidFrom]"));

        builder.HasKey(x => x.RecId);
        builder.Property(x => x.RecId).ValueGeneratedOnAdd();
        builder.Property(x => x.DataAreaId).HasMaxLength(4).IsRequired();
        builder.Property(x => x.ValidFrom).HasColumnType("date");
        builder.Property(x => x.ValidTo).HasColumnType("date");
        builder.Property(x => x.IsPrimary).HasDefaultValue(true);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.HcmWorker).WithMany(x => x.WorkerOrganizationAssignmentsV1)
            .HasForeignKey(x => x.HcmWorkerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.HcmManager).WithMany()
            .HasForeignKey(x => x.HcmManagerWorkerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Department).WithMany()
            .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Occupation).WithMany()
            .HasForeignKey(x => x.OccupationId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.DataAreaId, x.HcmWorkerId, x.IsPrimary, x.ValidFrom, x.ValidTo });
        builder.HasIndex(x => new { x.DataAreaId, x.HcmManagerWorkerId, x.ValidFrom, x.ValidTo });
        builder.HasIndex(x => new { x.DataAreaId, x.DepartmentId, x.ValidFrom, x.ValidTo });
        builder.HasIndex(x => new { x.DataAreaId, x.OccupationId, x.ValidFrom, x.ValidTo });
    }
}
