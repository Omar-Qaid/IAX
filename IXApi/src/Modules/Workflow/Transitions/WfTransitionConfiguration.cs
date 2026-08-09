using IAX.IXApi.Modules.Workflow.Transitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Transitions
{
    public class WfTransitionConfiguration : IEntityTypeConfiguration<WfTransition>
    {
        public void Configure(EntityTypeBuilder<WfTransition> builder)
        {
            builder.ToTable("WfTransitions");

            builder.HasKey(x => x.RecId);
            builder.Property(x => x.RecId)
                .HasColumnName("TransitionId")
                .ValueGeneratedOnAdd();

            // Configure relationships
            builder.HasOne(x => x.Process)
                .WithMany()
                .HasForeignKey(x => x.ProcessId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Activity)
                .WithMany()
                .HasForeignKey(x => x.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Variable)
                .WithMany()
                .HasForeignKey(x => x.VariableId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

