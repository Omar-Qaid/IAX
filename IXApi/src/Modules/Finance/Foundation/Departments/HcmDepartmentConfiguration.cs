using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.Departments
{
    public class HcmDepartmentConfiguration : IEntityTypeConfiguration<HcmDepartment>
    {
        public void Configure(EntityTypeBuilder<HcmDepartment> builder)
        {
            builder.ToTable("HcmDepartments");
        }
    }
}
