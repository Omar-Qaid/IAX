using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Processes
{
    public class WfUsersProcessConfiguration : IEntityTypeConfiguration<WfUsersProcess>
    {
        public void Configure(EntityTypeBuilder<WfUsersProcess> builder)
        {
            builder.ToTable("WfUsersProcesses");
            builder.HasKey(e => e.RecId);
            builder.Property(e => e.RecId).HasColumnName("UsersProcessesId");

            // Configure relationships
            builder.HasOne(x => x.Process)
                .WithMany(x => x.UsersProcesses)
                .HasForeignKey(x => x.ProcessId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

