using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeCategory
{
    public class OrgEmployeeCategoryGroupConfiguration : IEntityTypeConfiguration<OrgEmployeeCategoryGroup>
    {
        public void Configure(EntityTypeBuilder<OrgEmployeeCategoryGroup> builder)
        {
            builder.ToTable("OrgEmployeeCategoryGroups");
        }
    }
}

