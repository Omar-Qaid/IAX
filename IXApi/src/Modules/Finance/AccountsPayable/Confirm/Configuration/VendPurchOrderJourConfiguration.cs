using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// The posted purchase-order confirmation. Every lookup is by company and purchase order, which is
/// how the confirmation copy is retrieved after posting.
/// </summary>
public sealed class VendPurchOrderJourConfiguration : IEntityTypeConfiguration<VendPurchOrderJour>
{
    public void Configure(EntityTypeBuilder<VendPurchOrderJour> b)
    {
        b.ToTable("VendPurchOrderJour");
        b.HasIndex(x => new { x.DataAreaId, x.PurchId });
        b.Property(x => x.Qty).HasPrecision(18, 4);
        b.Property(x => x.Amount).HasPrecision(18, 4);
    }
}

/// <summary>Confirmation lines, read with their header's purchase order.</summary>
public sealed class VendPurchOrderTransConfiguration : IEntityTypeConfiguration<VendPurchOrderTrans>
{
    public void Configure(EntityTypeBuilder<VendPurchOrderTrans> b)
    {
        b.ToTable("VendPurchOrderTrans");
        b.HasIndex(x => new { x.DataAreaId, x.PurchId });
        b.Property(x => x.Qty).HasPrecision(18, 4);
        b.Property(x => x.PurchPrice).HasPrecision(18, 4);
        b.Property(x => x.PriceUnit).HasPrecision(18, 4);
        b.Property(x => x.LineNumber).HasPrecision(18, 4);
    }
}
