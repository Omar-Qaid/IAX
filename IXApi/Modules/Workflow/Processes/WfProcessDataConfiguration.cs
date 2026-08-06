using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Processes
{
    public class WfProcessDataConfiguration : IEntityTypeConfiguration<WfProcessData>
    {
        public void Configure(EntityTypeBuilder<WfProcessData> builder)
        {
            builder.ToTable("WfProcessData");

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.RecId)
                .HasColumnName("TaskID")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ActivityDetails)
                .HasColumnType("xml") // Map xml type correctly
                .IsRequired();

            // Configure relationships
            builder.HasOne(x => x.Assignment)
                .WithMany()
                .HasForeignKey(x => x.AssignmentID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

