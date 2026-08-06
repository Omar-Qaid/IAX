using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestMappingVariableConfiguration : IEntityTypeConfiguration<WfRequestMappingVariable>
    {
        public void Configure(EntityTypeBuilder<WfRequestMappingVariable> builder)
        {
            builder.ToTable("WfRequestMappingVariables");

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.RecId)
                .HasColumnName("MappingId")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.RequestControlId)
                .HasColumnName("RequestControlID");

            builder.Property(x => x.VariableId)
                .HasColumnName("VariableID");

            builder.Property(x => x.IsActive)
                .HasColumnName("Activated");

            // Ignore SortOrder as it doesn't exist in the DDL
            builder.Ignore(x => x.SortOrder);

            // Configure relationships
            builder.HasOne(x => x.RequestControl)
                .WithMany()
                .HasForeignKey(x => x.RequestControlId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Variable)
                .WithMany()
                .HasForeignKey(x => x.VariableId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

