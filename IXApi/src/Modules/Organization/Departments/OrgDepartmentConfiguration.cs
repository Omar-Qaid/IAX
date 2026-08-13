using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Departments
{
    public class OrgDepartmentConfiguration : IEntityTypeConfiguration<OrgDepartment>
    {
        public void Configure(EntityTypeBuilder<OrgDepartment> builder)
        {
            builder.ToTable("OrgDepartments");

            builder.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.HasIndex(x => x.Code).IsUnique();
        }
    }
}
