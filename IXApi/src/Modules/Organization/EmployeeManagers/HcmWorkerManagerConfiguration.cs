using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.HcmWorkerManagers
{
    public class HcmWorkerManagerConfiguration : IEntityTypeConfiguration<HcmWorkerManager>
    {
        public void Configure(EntityTypeBuilder<HcmWorkerManager> builder)
        {
            builder.ToTable("EmployeeManagers");

            // Composite key: one manager per management level per employee.
            builder.HasKey(x => new { x.EmployeeId, x.ManagementLevelId });

            builder.HasOne(x => x.Employee)
                .WithMany(e => e.Managers)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Manager)
                .WithMany()
                .HasForeignKey(x => x.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ManagementLevel)
                .WithMany()
                .HasForeignKey(x => x.ManagementLevelId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

