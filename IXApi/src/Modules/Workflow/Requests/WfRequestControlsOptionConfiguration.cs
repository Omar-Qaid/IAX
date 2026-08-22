using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestControlsOptionConfiguration : IEntityTypeConfiguration<WfRequestControlsOption>
    {
        public void Configure(EntityTypeBuilder<WfRequestControlsOption> builder)
        {
            builder.ToTable("WfRequestControlsOptions");

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.RecId)
                .HasColumnName("OptionId")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.RequestControlId)
                .HasColumnName("RequestControlId");

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

            builder.Property(x => x.Score)
                .HasColumnName("Score")
                .HasPrecision(18, 4);

            builder.Property(x => x.ExtendedProperties)
                .HasColumnName("ExtendedProperties");

            builder.Property(x => x.IsActive)
                .HasColumnName("IsActive");

            builder.HasOne(x => x.RequestControl)
                .WithMany()
                .HasForeignKey(x => x.RequestControlId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

