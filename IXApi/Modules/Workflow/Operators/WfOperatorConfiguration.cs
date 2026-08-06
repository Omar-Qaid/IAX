using IAX.IXApi.Modules.Workflow.Operators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Operators
{
    public class WfOperatorConfiguration : IEntityTypeConfiguration<WfOperator>
    {
        public void Configure(EntityTypeBuilder<WfOperator> builder)
        {
            builder.ToTable("WfOperators");
        }
    }
}
