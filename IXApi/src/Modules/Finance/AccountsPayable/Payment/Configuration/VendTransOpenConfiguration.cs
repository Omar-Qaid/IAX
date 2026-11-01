using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendTransOpenConfiguration : IEntityTypeConfiguration<VendTransOpen>
{
    public void Configure(EntityTypeBuilder<VendTransOpen> b)
    {
        b.ToTable("VendTransOpen");
        b.HasIndex(x => new { x.DataAreaId, x.RefRecId }).IsUnique();
        b.HasOne<VendTrans>().WithMany().HasForeignKey(x => x.RefRecId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.AmountCur).HasPrecision(18, 4);
        b.Property(x => x.AmountMst).HasPrecision(18, 4);
    }
}

public class VendSettlementConfiguration : IEntityTypeConfiguration<VendSettlement>
{
    public void Configure(EntityTypeBuilder<VendSettlement> b)
    {
        b.ToTable("VendSettlement");

        // The voucher is part of the key because a reversal writes an offsetting row for the same
        // invoice and payment pair; without it the reversal would collide with the settlement it
        // reverses.
        b.HasIndex(x => new { x.DataAreaId, x.InvoiceTransRecId, x.PaymentTransRecId, x.Voucher }).IsUnique();
        b.HasOne<VendTrans>().WithMany().HasForeignKey(x => x.InvoiceTransRecId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<VendTrans>().WithMany().HasForeignKey(x => x.PaymentTransRecId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.AmountCur).HasPrecision(18, 4);
        b.Property(x => x.AmountMst).HasPrecision(18, 4);
    }
}
