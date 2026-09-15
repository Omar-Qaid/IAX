using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Configuration
{
    public class SysBackgroundJobConfiguration : IEntityTypeConfiguration<SysBackgroundJob>
    {
        public void Configure(EntityTypeBuilder<SysBackgroundJob> builder)
        {
            builder.ToTable("BatchJobs");

            builder.HasIndex(e => e.Caption).IsUnique();
            builder.HasIndex(e => e.JobKey);
            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => e.TenantId);
            // Hot-path index for the scheduler poll: due, active, enabled jobs.
            builder.HasIndex(e => new { e.Status, e.IsEnabled, e.StartDateTime });

            builder.HasMany(e => e.Executions)
                   .WithOne(x => x.Job)
                   .HasForeignKey(x => x.JobId)
                   .OnDelete(DeleteBehavior.Cascade);

            // These D365-compatible fields are persisted as optional codes. There are no
            // corresponding lookup tables in the batch migration, so they must not be inferred
            // as relationships to the numeric RecId keys on the compatibility DTO types.
            builder.Ignore(e => e.GroupNavigation);
            builder.Ignore(e => e.ActivePeriodNavigation);
            builder.HasMany(e => e.RecurrenceCounts)
                .WithOne(count => count.BatchJob)
                .HasForeignKey(count => count.BatchJobId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
