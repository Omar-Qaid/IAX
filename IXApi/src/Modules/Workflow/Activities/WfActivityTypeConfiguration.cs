using IAX.IXApi.Modules.Workflow.Activities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityTypeConfiguration : IEntityTypeConfiguration<WfActivityType>
    {
        public void Configure(EntityTypeBuilder<WfActivityType> builder)
        {
            builder.ToTable("WfActivityTypes");

            builder.Property(x => x.RecId);
        }
    }
}

