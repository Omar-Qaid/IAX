using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Configuration
{
    /// <summary>
    /// EF Core configuration for the background job module entities.
    /// </summary>
    public class SysBackgroundJobConfiguration : IEntityTypeConfiguration<SysBackgroundJob>
    {
        public void Configure(EntityTypeBuilder<SysBackgroundJob> builder)
        {
            builder.ToTable("SysBackgroundJobs");

            builder.HasIndex(e => e.Name).IsUnique();
            builder.HasIndex(e => e.JobKey);
            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => e.TenantId);
            // Hot-path index for the scheduler poll: due, active, enabled jobs.
            builder.HasIndex(e => new { e.Status, e.IsEnabled, e.NextRunAt });

            builder.HasMany(e => e.Executions)
                   .WithOne(x => x.Job)
                   .HasForeignKey(x => x.JobId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class SysBackgroundJobExecutionConfiguration : IEntityTypeConfiguration<SysBackgroundJobExecution>
    {
        public void Configure(EntityTypeBuilder<SysBackgroundJobExecution> builder)
        {
            builder.ToTable("SysBackgroundJobExecutions");

            builder.HasIndex(e => e.JobId);
            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => new { e.JobId, e.CreatedAt });
        }
    }
}
