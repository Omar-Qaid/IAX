using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityMappingVariableConfiguration : IEntityTypeConfiguration<WfActivityMappingVariable>
    {
        public void Configure(EntityTypeBuilder<WfActivityMappingVariable> builder)
        {
            builder.ToTable("WfActivityMappingVariables");

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.RecId)
                .HasColumnName("MappingId")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ActivityControlId)
                .HasColumnName("ActivityControlID");

            builder.Property(x => x.VariableId)
                .HasColumnName("VariableID");

            builder.Property(x => x.IsActive)
                .HasColumnName("Activated");

            // Ignore VariableOrder as it doesn't exist in the DDL
            builder.Ignore(x => x.VariableOrder);

            // Configure relationships
            builder.HasOne(x => x.ActivityControl)
                .WithMany()
                .HasForeignKey(x => x.ActivityControlId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Variable)
                .WithMany()
                .HasForeignKey(x => x.VariableId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

