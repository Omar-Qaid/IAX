using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Features.HcmWorkerCategory
{
    public class HcmWorkerCategoryConfiguration : IEntityTypeConfiguration<HcmWorkerCategory>
    {
        public void Configure(EntityTypeBuilder<HcmWorkerCategory> builder)
        {
            builder.ToTable("OrgEmployeeCategories");

        }
    }
}

