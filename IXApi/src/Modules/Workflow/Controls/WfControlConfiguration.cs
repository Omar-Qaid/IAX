using IAX.IXApi.Modules.Workflow.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Controls
{
    public class WfControlConfiguration : IEntityTypeConfiguration<WfControl>
    {
        public void Configure(EntityTypeBuilder<WfControl> builder)
        {
            builder.ToTable("WfControls");
        }
    }
}
