using IAX.IXApi.Modules.Workflow.Activities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityConfiguration : IEntityTypeConfiguration<WfActivity>
    {
        public void Configure(EntityTypeBuilder<WfActivity> builder)
        {
            builder.ToTable("WfActivities");

            // Configure relationships
            builder.HasOne(x => x.ActivityType)
                .WithMany()
                .HasForeignKey(x => x.ActivityTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Performer)
                .WithMany()
                .HasForeignKey(x => x.PerformerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Step)
                .WithMany()
                .HasForeignKey(x => x.StepId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SysNotificationTemplate)
                .WithMany()
                .HasForeignKey(x => x.SysNotificationTemplateId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
