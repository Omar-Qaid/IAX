using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using IAX.IXApi.Shared.Domain.Reporting;

namespace IAX.IXApi.Infrastructure.Persistence.Configuration.Reporting;

public sealed class ReportTemplateVersionConfiguration : IEntityTypeConfiguration<ReportTemplateVersion>
{
    public void Configure(EntityTypeBuilder<ReportTemplateVersion> builder)
    {
        builder.ToTable("ReportTemplateVersions");
        builder.Property(item => item.RecId).HasColumnName("TemplateVersionId");
        builder.Property(item => item.TemplateJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(item => item.PublishedBy).HasMaxLength(450);
        builder.HasIndex(item => new { item.DataAreaId, item.TemplateId, item.VersionNo }).IsUnique();
        builder.HasOne(item => item.Template)
            .WithMany(item => item.Versions)
            .HasForeignKey(item => item.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
