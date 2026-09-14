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
        builder.Property(x => x.Name).HasColumnName("ControlLabel");
        builder.Property(x => x.NameAlias).HasColumnName("ControlLabelAR");
        builder.Property(x => x.ValueAlias).HasColumnName("ControlValueAR");
        builder.Property(x => x.Value).HasColumnName("ControlValueEN");
        builder.Property(x => x.ControlValue).HasMaxLength(255);
        // These legacy columns remain required in existing ERM databases.
        builder.Property<bool>("UsedAsCriteria").HasDefaultValue(false);
        builder.Property<long>("RelatedObjectId").HasDefaultValue(0L);
        builder.Property(x => x.SortOrder).HasColumnName("ControlOrder");
    }
}
