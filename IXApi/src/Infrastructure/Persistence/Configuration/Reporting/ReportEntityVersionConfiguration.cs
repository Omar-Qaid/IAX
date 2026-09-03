using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using IAX.IXApi.Shared.Domain.Reporting;

namespace IAX.IXApi.Infrastructure.Persistence.Configuration.Reporting;

public sealed class ReportEntityVersionConfiguration : IEntityTypeConfiguration<ReportEntityVersion>
{
    public void Configure(EntityTypeBuilder<ReportEntityVersion> builder)
    {
        builder.ToTable("ReportEntityVersions");
        builder.Property(item => item.RecId).HasColumnName("ReportEntityVersionId");
        builder.Property(item => item.SelectedBy).HasMaxLength(450).IsRequired();
        builder.HasIndex(item => new { item.DataAreaId, item.RefTableId, item.RefRecId, item.TemplateId }).IsUnique();
        builder.HasOne(item => item.Template).WithMany().HasForeignKey(item => item.TemplateId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.TemplateVersion).WithMany().HasForeignKey(item => item.TemplateVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}
