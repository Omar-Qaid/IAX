using IAX.IXApi.Modules.Workflow.Priorities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Priorities
{
    public class WfPriorityConfiguration : IEntityTypeConfiguration<WfPriority>
    {
        public void Configure(EntityTypeBuilder<WfPriority> builder)
        {
            builder.ToTable("WfPriorities");
        }
    }
}
