using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments;

public class HcmWorkerShowroomAssignmentConfiguration : IEntityTypeConfiguration<HcmWorkerShowroomAssignment>
{
    public void Configure(EntityTypeBuilder<HcmWorkerShowroomAssignment> builder)
    {
        builder.ToTable("HcmWorkerShowroomAssignments", t => t.HasCheckConstraint("CK_HcmWorkerShowroomAssignments_Dates", "[ValidTo] IS NULL OR [ValidTo] > [ValidFrom]"));
        builder.Property(x => x.DataAreaId).HasMaxLength(4).IsRequired();
        builder.HasKey(x => x.RecId);
        builder.Property(x => x.RecId).ValueGeneratedOnAdd();
        builder.Property(x => x.ValidFrom).HasColumnType("date");
        builder.Property(x => x.ValidTo).HasColumnType("date");
        builder.Property(x => x.IsPrimary).HasDefaultValue(true);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.HcmWorker)
            .WithMany(x => x.WorkerShowroomAssignments)
            .HasForeignKey(x => x.HcmWorkerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.HcmShowroom)
            .WithMany(x => x.WorkerShowroomAssignments)
            .HasForeignKey(x => x.HcmShowroomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.DataAreaId, x.HcmWorkerId, x.IsPrimary, x.ValidFrom, x.ValidTo });
        builder.HasIndex(x => new { x.HcmWorkerId, x.ValidFrom, x.ValidTo });
        builder.HasIndex(x => new { x.HcmShowroomId, x.ValidFrom, x.ValidTo });
    }
}
