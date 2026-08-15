using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityControlsValidationConfiguration : IEntityTypeConfiguration<WfActivityControlsValidation>
    {
        public void Configure(EntityTypeBuilder<WfActivityControlsValidation> builder)
        {
            builder.ToTable("WfActivityControlsValidations");

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.RecId)
                .HasColumnName("ValidationId")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ActivityControlId)
                .HasColumnName("ActivityControlId");

            builder.Property(x => x.ValidationType)
                .HasColumnName("ValidationType")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.ValidationExpression)
                .HasColumnName("ValidationExpression")
                .HasMaxLength(2000);

            builder.Property(x => x.Operator)
                .HasColumnName("Operator")
                .HasMaxLength(100);

            builder.Property(x => x.Value)
                .HasColumnName("Value")
                .HasMaxLength(1000);

            builder.Property(x => x.MaskInput)
                .HasColumnName("MaskInput")
                .HasMaxLength(1000);

            builder.Property(x => x.ErrorMessage)
                .HasColumnName("ErrorMessage")
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(x => x.Severity)
                .HasColumnName("Severity")
                .HasMaxLength(50)
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

