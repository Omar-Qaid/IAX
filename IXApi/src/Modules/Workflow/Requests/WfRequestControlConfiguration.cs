using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestControlConfiguration : IEntityTypeConfiguration<WfRequestControl>
    {
        public void Configure(EntityTypeBuilder<WfRequestControl> builder)
        {
            builder.ToTable("WfRequestControls");

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.RecId)
                .HasColumnName("RequestControlId")
                .ValueGeneratedOnAdd();

            // Configure relationships
            builder.HasOne(x => x.Control)
                .WithMany()
                .HasForeignKey(x => x.ControlId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Process)
                .WithMany()
                .HasForeignKey(x => x.ProcessId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

