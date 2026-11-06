using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// The posted product receipt. The source-document reference is configured with the other purchase
/// documents; this adds the lookups the receipt and invoice-matching flows use.
/// </summary>
public sealed class VendPackingSlipJourConfiguration : IEntityTypeConfiguration<VendPackingSlipJour>
{
    public void Configure(EntityTypeBuilder<VendPackingSlipJour> b)
    {
        b.ToTable("VendPackingSlipJour");
        b.HasIndex(x => new { x.DataAreaId, x.PurchId });
        b.HasIndex(x => new { x.DataAreaId, x.InternalPackingSlipId });
        b.Property(x => x.Qty).HasPrecision(18, 4);
    }
}

/// <summary>
/// Product receipt lines. Invoice matching reaches them by the originating purchase order and by
/// the inventory transaction of the line being invoiced.
/// </summary>
public sealed class VendPackingSlipTransConfiguration : IEntityTypeConfiguration<VendPackingSlipTrans>
{
    public void Configure(EntityTypeBuilder<VendPackingSlipTrans> b)
    {
        b.ToTable("VendPackingSlipTrans");
        b.HasIndex(x => new { x.DataAreaId, x.OrigPurchId });
        // The column carried no length, so indexing it would otherwise have EF re-type it to
        // nvarchar(450) - the largest indexable key. Declare the real domain length instead.
        b.Property(x => x.InventTransId).HasMaxLength(FieldLengths.InventTransId);
        b.HasIndex(x => new { x.DataAreaId, x.InventTransId });
    }
}
