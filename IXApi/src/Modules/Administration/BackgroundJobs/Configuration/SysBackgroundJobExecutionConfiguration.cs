using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Configuration
{
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