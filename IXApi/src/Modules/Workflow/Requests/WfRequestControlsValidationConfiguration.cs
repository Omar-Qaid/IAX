using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestControlsValidationConfiguration : IEntityTypeConfiguration<WfRequestControlsValidation>
    {
        public void Configure(EntityTypeBuilder<WfRequestControlsValidation> builder)
        {
            builder.ToTable("WfRequestControlsValidations");

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.RecId)
                .HasColumnName("ValidationId")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.RequestControlId)
                .HasColumnName("RequestControlId");

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

            builder.Property(x => x.ErrorMessageAlias)
                .HasColumnName("ErrorMessageAlias")
                .HasMaxLength(1000);

            builder.Property(x => x.Severity)
                .HasColumnName("Severity")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.SortOrder)
                .HasColumnName("SortOrder");

            builder.Property(x => x.IsActive)
                .HasColumnName("IsActive");

            // Configure relationships
            builder.HasOne(x => x.RequestControl)
                .WithMany()
                .HasForeignKey(x => x.RequestControlId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

