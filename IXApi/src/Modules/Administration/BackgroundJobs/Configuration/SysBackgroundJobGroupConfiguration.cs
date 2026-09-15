using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Configuration;

public sealed class SysBackgroundJobGroupConfiguration : IEntityTypeConfiguration<SysBackgroundJobGroup>
{
    public void Configure(EntityTypeBuilder<SysBackgroundJobGroup> builder)
    {
        builder.ToTable("SysBackgroundJobGroup");
        builder.HasIndex(group => new { group.DataAreaId, group.GroupCode }).IsUnique();
        builder.Property(group => group.GroupCode).HasMaxLength(10).IsRequired();
        builder.Property(group => group.Description).HasMaxLength(60);
    }
}
