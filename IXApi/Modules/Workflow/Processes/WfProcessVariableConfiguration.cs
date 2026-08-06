using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Processes
{
    public class WfProcessVariableConfiguration : IEntityTypeConfiguration<WfProcessVariable>
    {
        public void Configure(EntityTypeBuilder<WfProcessVariable> builder)
        {
            builder.ToTable("WfProcessVariables");

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.RecId)
                .HasColumnName("ProcessVariableId")
                .ValueGeneratedOnAdd();

            // Ignore properties not in DDL
            builder.Ignore(x => x.IsActive);
            builder.Ignore(x => x.RowVersion);
            builder.Ignore(x => x.SortOrder);

            // Configure relationships
            builder.HasOne(x => x.Request)
                .WithMany()
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Variable)
                .WithMany()
                .HasForeignKey(x => x.VariableId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

