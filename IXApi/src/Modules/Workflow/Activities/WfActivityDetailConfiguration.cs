using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Activities;

public sealed class WfActivityDetailConfiguration : IEntityTypeConfiguration<WfActivityDetail>
{
    public void Configure(EntityTypeBuilder<WfActivityDetail> builder)
    {
        builder.ToTable("WfActivityDetails");
        builder.HasKey(x => x.RecId);
        builder.Property(x => x.RecId)
            .HasColumnName("ActivityDetailID")
            .ValueGeneratedOnAdd();
        builder.Property(x => x.ProcessId).HasColumnName("TaskID");
        builder.Property(x => x.SortOrder).HasColumnName("ControlOrder");
    }
}
