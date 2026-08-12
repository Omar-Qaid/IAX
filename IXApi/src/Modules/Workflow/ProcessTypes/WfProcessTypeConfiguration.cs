using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.ProcessTypes
{
    public class WfProcessTypeConfiguration : IEntityTypeConfiguration<WfProcessType>
    {
        public void Configure(EntityTypeBuilder<WfProcessType> builder)
        {
            builder.ToTable("WfProcessTypes");
        }
    }
}
