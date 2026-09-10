using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Workflow.Requests;

public class WfRequestDetailConfiguration : IEntityTypeConfiguration<WfRequestDetail>
{
    public void Configure(EntityTypeBuilder<WfRequestDetail> builder)
    {
        builder.Property(item => item.ControlLabel).IsRequired().HasDefaultValue(string.Empty);
        builder.Property(item => item.ControlLabelAlias).IsRequired().HasDefaultValue(string.Empty);
    }
}
