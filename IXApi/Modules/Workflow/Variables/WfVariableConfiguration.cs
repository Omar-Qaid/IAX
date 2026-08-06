using IAX.IXApi.Modules.Workflow.Variables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Variables
{
    public class WfVariableConfiguration : IEntityTypeConfiguration<WfVariable>
    {
        public void Configure(EntityTypeBuilder<WfVariable> builder)
        {
            builder.ToTable("WfVariables");

            // Configure relationships
            builder.HasOne(x => x.DataType)
                .WithMany()
                .HasForeignKey(x => x.DataTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Process)
                .WithMany()
                .HasForeignKey(x => x.ProcessId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
