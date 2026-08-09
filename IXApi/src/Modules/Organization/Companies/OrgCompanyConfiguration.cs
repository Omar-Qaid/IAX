using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Organization.Companies
{
    public class OrgCompanyConfiguration : IEntityTypeConfiguration<OrgCompany>
    {
        public void Configure(EntityTypeBuilder<OrgCompany> builder)
        {
            builder.ToTable("OrgCompanies");

            builder.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.NameAR)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.DescriptionAR)
                .HasMaxLength(1000);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.PrimaryEmail).HasMaxLength(200);
            builder.Property(x => x.NotificationEmail).HasMaxLength(200);
            builder.Property(x => x.IdentityUrl).HasMaxLength(500);
            builder.Property(x => x.LogoUrl).HasMaxLength(500);
            builder.Property(x => x.ColorTheme).HasMaxLength(50);
            builder.Property(x => x.ColorMode).HasMaxLength(20);

            builder.HasIndex(x => x.Code).IsUnique();
        }
    }
}
