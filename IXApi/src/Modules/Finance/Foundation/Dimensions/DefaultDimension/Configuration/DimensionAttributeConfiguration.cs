using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.Dimensions.DefaultDimension.Configuration
{
    /// <summary>
    /// A financial dimension. The entity existed with no configuration, so the table was never
    /// created and no dimension could be defined.
    /// </summary>
    public class DimensionAttributeConfiguration : IEntityTypeConfiguration<DimensionAttribute>
    {
        public void Configure(EntityTypeBuilder<DimensionAttribute> builder)
        {
            builder.ToTable("DimensionAttribute");
            // A dimension is named once per company.
            builder.HasIndex(x => new { x.DataAreaId, x.Name }).IsUnique();
        }
    }

    /// <summary>A value of a financial dimension, such as the ADMIN department.</summary>
    public class DimensionAttributeValueConfiguration : IEntityTypeConfiguration<DimensionAttributeValue>
    {
        public void Configure(EntityTypeBuilder<DimensionAttributeValue> builder)
        {
            builder.ToTable("DimensionAttributeValue");
            // One value per dimension, per company.
            builder.HasIndex(x => new { x.DataAreaId, x.DimensionAttribute, x.DisplayValue }).IsUnique();
            builder.HasOne<DimensionAttribute>().WithMany()
                .HasForeignKey(x => x.DimensionAttribute).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
