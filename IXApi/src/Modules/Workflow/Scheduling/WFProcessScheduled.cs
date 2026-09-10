using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Workflow.Processes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Scheduling;

// Explicit company predicates are mandatory: this configuration is also discovered by global workers.
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

public sealed class ProcessScheduleDto
{
    public bool Enabled { get; set; }
    public string Frequency { get; set; } = "daily";
    public DateTime StartsAt { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string SourceType { get; set; } = "employee";
    public string SourceTable { get; set; } = "";
    public string SourceRecord { get; set; } = "";
    public List<ProcessScheduleMappingDto> Mappings { get; set; } = [];
    // Optimistic concurrency token; account/company and execution state never come from the client.
    public string? Version { get; set; }
}

public sealed class ProcessScheduleMappingDto
{
    public string TargetControlId { get; set; } = "";
    public string SourceField { get; set; } = "";
}
