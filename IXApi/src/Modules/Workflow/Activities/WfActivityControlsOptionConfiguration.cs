using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityControlsOptionConfiguration : IEntityTypeConfiguration<WfActivityControlsOption>
    {
        public void Configure(EntityTypeBuilder<WfActivityControlsOption> builder)
        {
            builder.ToTable("WfActivityControlsOptions");

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.RecId)
                .HasColumnName("OptionId")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ActivityControlId)
                .HasColumnName("ActivityControlId");

            builder.Property(x => x.Value)
                .HasColumnName("Value")
                .HasMaxLength(1000)
                .IsRequired();



            builder.Property(x => x.Name)
                .HasColumnName("Name")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.SortOrder)
                .HasColumnName("SortOrder");

            builder.Property(x => x.IsActive)
                .HasColumnName("IsActive");

            builder.HasOne(x => x.ActivityControl)
                .WithMany()
                .HasForeignKey(x => x.ActivityControlId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

