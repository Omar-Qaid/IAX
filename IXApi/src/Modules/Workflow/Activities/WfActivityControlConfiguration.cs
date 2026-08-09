using IAX.IXApi.Modules.Workflow.Activities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityControlConfiguration : IEntityTypeConfiguration<WfActivityControl>
    {
        public void Configure(EntityTypeBuilder<WfActivityControl> builder)
        {
            builder.ToTable("WfActivityControls");

            builder.HasKey(x => x.RecId);
            builder.Property(x => x.RecId)
                .HasColumnName("ActivityControlId")
                .ValueGeneratedOnAdd();

            // Configure relationships
            builder.HasOne(x => x.Activity)
                .WithMany()
                .HasForeignKey(x => x.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Control)
                .WithMany()
                .HasForeignKey(x => x.ControlId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

