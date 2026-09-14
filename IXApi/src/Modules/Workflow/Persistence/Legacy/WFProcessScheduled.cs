using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Workflow.Processes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Scheduling;

// Archive-only mapping. Preserve the CLR namespace and table so retiring the feature does not
// delete saved configuration/history or cause EF to generate a destructive schema migration.
public sealed class WFProcessScheduled
{
    public long RecId { get; set; }
    public long ProcessId { get; set; }
    public string DataAreaId { get; set; } = "";
    public string ExecutionUserId { get; set; } = "";
    public string OwnerAccountId { get; set; } = "";
    public bool Enabled { get; set; }
    public string ConfigurationJson { get; set; } = "{}";
    public DateTime? NextRunAt { get; set; }
    public DateTime? LastRunAt { get; set; }
    public long? LastRequestId { get; set; }
    public long BackgroundJobId { get; set; }
    public SysBackgroundJob BackgroundJob { get; set; } = null!;
    public byte[] RowVersion { get; set; } = [];
}

public sealed class WFProcessScheduledConfiguration : IEntityTypeConfiguration<WFProcessScheduled>
{
    public void Configure(EntityTypeBuilder<WFProcessScheduled> builder)
    {
        builder.ToTable("WFProcessScheduled");
        builder.HasKey(x => x.RecId);
        builder.Property(x => x.DataAreaId).HasMaxLength(10).IsRequired();
        builder.Property(x => x.ExecutionUserId).HasMaxLength(256).IsRequired();
        builder.Property(x => x.OwnerAccountId).HasMaxLength(256).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.DataAreaId, x.ProcessId }).IsUnique();
        builder.HasIndex(x => x.BackgroundJobId).IsUnique();
        builder.HasOne(x => x.BackgroundJob).WithMany().HasForeignKey(x => x.BackgroundJobId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WfProcess>().WithMany().HasForeignKey(x => x.ProcessId).OnDelete(DeleteBehavior.Restrict);
    }
}

