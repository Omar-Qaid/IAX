using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;

/// <summary>One seeder run. <see cref="TableName"/> holds the key <c>{id}:v{version}:{scope}</c>.</summary>
[Table("SysDataSeedLogs")]
public class SysDataSeedLog
{
    [Key]
    public int RecId { get; set; }
    public string TableName { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? SeederId { get; set; }
    public int Version { get; set; } = 1;
    public string Scope { get; set; } = "global";
    public string Outcome { get; set; } = "Success";
    public string Policy { get; set; } = "Legacy";
    public long DurationMs { get; set; }
    public string? Error { get; set; }
}

public sealed class SysDataSeedLogConfiguration : IEntityTypeConfiguration<SysDataSeedLog>
{
    public void Configure(EntityTypeBuilder<SysDataSeedLog> builder)
    {
        builder.Property(x => x.SeederId).HasMaxLength(200);
        builder.Property(x => x.Scope).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Outcome).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Policy).HasMaxLength(16).IsRequired();
        // A run-once seeder may succeed only once per version and scope.
        builder.HasIndex(x => new { x.SeederId, x.Version, x.Scope }).IsUnique()
            .HasFilter("[SeederId] IS NOT NULL AND [Policy] = 'RunOnce' AND [Outcome] = 'Success'");
    }
}
