using IAX.IXApi.Modules.Workflow.Processes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Processes
{
    public class WfProcessConfiguration : IEntityTypeConfiguration<WfProcess>
    {
        public void Configure(EntityTypeBuilder<WfProcess> builder)
        {
            builder.ToTable("WfProcesses");
        }
    }
}
