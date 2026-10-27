using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestVariableConfiguration : IEntityTypeConfiguration<WfRequestVariable>
    {
        public void Configure(EntityTypeBuilder<WfRequestVariable> builder)
        {
            builder.ToTable("WfRequestVariables");

            builder.HasKey(x => x.RecId);
            builder.HasIndex(x => new { x.RequestId, x.VariableId }).IsUnique();

            builder.Property(x => x.RequestId)
                .HasColumnName("RequestId")
                .ValueGeneratedNever();

            // Configure relationships
            builder.HasOne(x => x.Request)
                .WithMany()
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Variable)
                .WithMany()
                .HasForeignKey(x => x.VariableId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
