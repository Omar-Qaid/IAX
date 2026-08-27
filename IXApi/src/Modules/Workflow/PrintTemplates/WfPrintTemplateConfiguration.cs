using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.PrintTemplates;

public sealed class WfPrintTemplateConfiguration : IEntityTypeConfiguration<WfPrintTemplate>
{
    public void Configure(EntityTypeBuilder<WfPrintTemplate> builder)
    {
        builder.ToTable("WfPrintTemplates");
        builder.Property(item => item.RecId).HasColumnName("TemplateId");
        builder.Property(item => item.Code).HasMaxLength(50).IsRequired();
        builder.Property(item => item.Name).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Description).HasMaxLength(1000);
        builder.Property(item => item.PageSize).HasMaxLength(20).IsRequired();
        builder.Property(item => item.Orientation).HasMaxLength(20).IsRequired();
        builder.Property(item => item.Language).HasMaxLength(10).IsRequired();
        builder.Property(item => item.Status).HasConversion<byte>();

        builder.HasIndex(item => new { item.DataAreaId, item.ProcessId, item.Code }).IsUnique();
        builder.HasIndex(item => new { item.DataAreaId, item.ProcessId, item.IsDefault })
            .IsUnique()
            .HasFilter("[IsDefault] = 1 AND [IsDeleted] = 0 AND [IsActive] = 1");

        builder.HasOne(item => item.Process)
            .WithMany()
            .HasForeignKey(item => item.ProcessId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.CurrentVersion)
            .WithMany()
            .HasForeignKey(item => item.CurrentVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
