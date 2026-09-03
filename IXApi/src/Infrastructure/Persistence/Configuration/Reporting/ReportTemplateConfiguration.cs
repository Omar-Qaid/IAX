using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using IAX.IXApi.Shared.Domain.Reporting;

namespace IAX.IXApi.Infrastructure.Persistence.Configuration.Reporting;

public sealed class ReportTemplateConfiguration : IEntityTypeConfiguration<ReportTemplate>
{
    public void Configure(EntityTypeBuilder<ReportTemplate> builder)
    {
        builder.ToTable("ReportTemplates");
        builder.Property(item => item.RecId).HasColumnName("TemplateId");
        builder.Property(item => item.Code).HasMaxLength(50).IsRequired();
        builder.Property(item => item.Name).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Description).HasMaxLength(1000);
        builder.Property(item => item.PageSize).HasMaxLength(20).IsRequired();
        builder.Property(item => item.Orientation).HasMaxLength(20).IsRequired();
        builder.Property(item => item.Language).HasMaxLength(10).IsRequired();
        builder.Property(item => item.Status).HasConversion<byte>();

        builder.HasIndex(item => new { item.DataAreaId, item.RefTableId, item.RefRecId, item.Code }).IsUnique();
        builder.HasIndex(item => new { item.DataAreaId, item.RefTableId, item.RefRecId, item.IsDefault })
            .IsUnique()
            .HasFilter("[IsDefault] = 1 AND [IsDeleted] = 0 AND [IsActive] = 1");

        builder.HasOne(item => item.CurrentVersion)
            .WithMany()
            .HasForeignKey(item => item.CurrentVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
