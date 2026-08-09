using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Execution
{
    public class WfAssignmentConfiguration : IEntityTypeConfiguration<WfAssignment>
    {
        public void Configure(EntityTypeBuilder<WfAssignment> builder)
        {
            builder.ToTable("WfAssignments");

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.RecId)
                .HasColumnName("AssignmentID")
                .ValueGeneratedOnAdd();

            // Configure relationships
            builder.HasOne(x => x.Request)
                .WithMany()
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Activity)
                .WithMany()
                .HasForeignKey(x => x.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

