using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Features.HcmWorkerCategory
{
    public class HcmWorkerCategoryGroupConfiguration : IEntityTypeConfiguration<HcmWorkerCategoryGroup>
    {
        public void Configure(EntityTypeBuilder<HcmWorkerCategoryGroup> builder)
        {
            builder.ToTable("EmployeeCategoryGroups");
        }
    }
}

