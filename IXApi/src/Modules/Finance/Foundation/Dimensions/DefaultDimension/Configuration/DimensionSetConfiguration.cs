using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace IAX.IXApi.Modules.Finance.Foundation.Dimensions.DefaultDimension.Configuration;
public sealed class DimensionAttributeSetConfiguration : IEntityTypeConfiguration<DimensionAttributeSet>
{
 public void Configure(EntityTypeBuilder<DimensionAttributeSet> b) { b.ToTable("DimensionAttributeSet"); }
}
public sealed class DimensionAttributeSetItemConfiguration : IEntityTypeConfiguration<DimensionAttributeSetItem>
{
 public void Configure(EntityTypeBuilder<DimensionAttributeSetItem> b)
 {
  b.ToTable("DimensionAttributeSetItem");
  b.HasIndex(x => new { x.DataAreaId, x.DimensionAttributeSet, x.DimensionAttribute }).IsUnique();
  b.HasOne<DimensionAttributeSet>().WithMany().HasForeignKey(x => x.DimensionAttributeSet).OnDelete(DeleteBehavior.Restrict);
  b.HasOne<DimensionAttribute>().WithMany().HasForeignKey(x => x.DimensionAttribute).OnDelete(DeleteBehavior.Restrict);
 }
}
public sealed class DimensionAttributeValueSetConfiguration : IEntityTypeConfiguration<DimensionAttributeValueSet>
{
 public void Configure(EntityTypeBuilder<DimensionAttributeValueSet> b) { b.ToTable("DimensionAttributeValueSet"); }
}
public sealed class DimensionAttributeValueSetItemConfiguration : IEntityTypeConfiguration<DimensionAttributeValueSetItem>
{
 public void Configure(EntityTypeBuilder<DimensionAttributeValueSetItem> b)
 {
  b.ToTable("DimensionAttributeValueSetItem");
  b.HasIndex(x => new { x.DataAreaId, x.DimensionAttributeValueSet, x.DimensionAttributeValue }).IsUnique();
  b.HasOne<DimensionAttributeValueSet>().WithMany().HasForeignKey(x => x.DimensionAttributeValueSet).OnDelete(DeleteBehavior.Restrict);
  b.HasOne<DimensionAttributeValue>().WithMany().HasForeignKey(x => x.DimensionAttributeValue).OnDelete(DeleteBehavior.Restrict);
 }
}
