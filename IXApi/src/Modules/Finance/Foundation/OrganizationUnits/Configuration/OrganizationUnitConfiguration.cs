using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits
{
    public class OrganizationUnitConfiguration : IEntityTypeConfiguration<OrganizationUnit>
    {
        public void Configure(EntityTypeBuilder<OrganizationUnit> builder)
        {
            builder.ToTable("OrganizationUnits", t => t.HasCheckConstraint("CK_OrganizationUnits_Dates", "[ValidTo] IS NULL OR [ValidTo] > [ValidFrom]"));
            builder.Property(x => x.DataAreaId).HasMaxLength(4).IsRequired();
            builder.HasKey(x => x.RecId);
            builder.Property(x => x.RecId).ValueGeneratedOnAdd();

            builder.Property(x => x.Code).HasMaxLength(50).IsUnicode().IsRequired();
            builder.Property(x => x.Name).HasMaxLength(200).IsUnicode().IsRequired();
            builder.Property(x => x.NameAlias).HasMaxLength(200).IsUnicode();
            builder.Property(x => x.OrganizationUnitType).HasColumnType("tinyint").IsRequired();
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.HasIndex(x => new { x.DataAreaId, x.Code }).IsUnique();

            builder.HasOne(x => x.ParentOrganizationUnit)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentOrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
