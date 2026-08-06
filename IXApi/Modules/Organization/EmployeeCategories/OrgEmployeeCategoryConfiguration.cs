using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeCategory
{
    public class OrgEmployeeCategoryConfiguration : IEntityTypeConfiguration<OrgEmployeeCategory>
    {
        public void Configure(EntityTypeBuilder<OrgEmployeeCategory> builder)
        {
            builder.ToTable("OrgEmployeeCategories");

        }
    }
}

