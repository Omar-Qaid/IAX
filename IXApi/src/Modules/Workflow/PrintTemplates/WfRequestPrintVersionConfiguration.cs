using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.PrintTemplates;

public sealed class WfRequestPrintVersionConfiguration : IEntityTypeConfiguration<WfRequestPrintVersion>
{
    public void Configure(EntityTypeBuilder<WfRequestPrintVersion> builder)
    {
        builder.ToTable("WfRequestPrintVersions");
        builder.Property(item => item.RecId).HasColumnName("RequestPrintVersionId");
        builder.Property(item => item.SelectedBy).HasMaxLength(450).IsRequired();
        builder.HasIndex(item => new { item.DataAreaId, item.RequestId, item.TemplateId }).IsUnique();
        builder.HasOne(item => item.Request).WithMany().HasForeignKey(item => item.RequestId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.Template).WithMany().HasForeignKey(item => item.TemplateId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.TemplateVersion).WithMany().HasForeignKey(item => item.TemplateVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}
