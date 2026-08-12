using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    public class WfPerformerTypeConfiguration : IEntityTypeConfiguration<WfPerformerType>
    {
        public void Configure(EntityTypeBuilder<WfPerformerType> builder)
        {
            builder.ToTable("WfPerformerType");
        }
    }
}
