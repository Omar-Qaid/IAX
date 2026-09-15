using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Configuration;

public sealed class BatchJobActivePeriodConfiguration : IEntityTypeConfiguration<SysBackgroundJobActivePeriod>
{
    public void Configure(EntityTypeBuilder<SysBackgroundJobActivePeriod> builder)
    {
        builder.ToTable("BatchJobActivePeriod");
        builder.Property(period => period.Code).HasColumnName("ID").HasMaxLength(10).IsRequired();
        builder.HasIndex(period => new { period.DataAreaId, period.Code }).IsUnique();
    }
}
