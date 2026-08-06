using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Steps
{
    public class WfStepConfiguration : IEntityTypeConfiguration<WfStep>
    {
        public void Configure(EntityTypeBuilder<WfStep> builder)
        {
            builder.ToTable("WfSteps");

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.RecId)
                .HasColumnName("StepId")
                .ValueGeneratedOnAdd();

            // Configure relationships
            builder.HasOne(x => x.Process)
                .WithMany()
                .HasForeignKey(x => x.ProcessId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

