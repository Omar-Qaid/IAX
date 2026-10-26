using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

public sealed class SysDataSeedLogConfiguration : IEntityTypeConfiguration<SysDataSeedLog>
{
    public void Configure(EntityTypeBuilder<SysDataSeedLog> builder)
    {
        builder.Property(x => x.SeederId).HasMaxLength(200);
        builder.Property(x => x.Scope).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Outcome).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Policy).HasMaxLength(16).IsRequired();
        builder.HasIndex(x => new { x.SeederId, x.Version, x.Scope })
            .IsUnique()
            .HasFilter("[SeederId] IS NOT NULL AND [Policy] = 'RunOnce' AND [Outcome] = 'Success'");
    }
}
