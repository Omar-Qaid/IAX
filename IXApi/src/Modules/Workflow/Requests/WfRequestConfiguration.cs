using IAX.IXApi.Modules.Workflow.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestConfiguration : IEntityTypeConfiguration<WfRequest>
    {
        public void Configure(EntityTypeBuilder<WfRequest> builder)
        {
            builder.ToTable("WfRequests");
            builder.Property(x => x.RequestForType).HasColumnType("tinyint");
            builder.HasOne(x => x.RequestForHcmWorker).WithMany()
                .HasForeignKey(x => x.RequestForHcmWorkerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany()
                .HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HcmWorkerAssignment).WithMany()
                .HasForeignKey(x => x.HcmWorkerAssignmentId).OnDelete(DeleteBehavior.Restrict);

            // Configure relationships
            builder.HasOne(x => x.Process)
                .WithMany()
                .HasForeignKey(x => x.ProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
