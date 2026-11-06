using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// The vendor subledger transaction. Indexed on the keys the payable flows actually filter by -
/// the vendor account for the open balance and settlement, and the invoice and voucher for
/// document lookups - and tied to the vendor it belongs to.
/// </summary>
public sealed class VendTransConfiguration : IEntityTypeConfiguration<VendTrans>
{
    public void Configure(EntityTypeBuilder<VendTrans> b)
    {
        b.ToTable("VendTrans");
        b.HasIndex(x => new { x.DataAreaId, x.AccountNum });
        b.HasIndex(x => new { x.DataAreaId, x.Invoice });
        b.HasIndex(x => new { x.DataAreaId, x.Voucher });
        b.HasOne<VendTable>().WithMany().HasForeignKey(x => x.AccountNum)
            .HasPrincipalKey(x => x.AccountNum).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.AmountCur).HasPrecision(18, 4);
        b.Property(x => x.AmountMst).HasPrecision(18, 4);
        b.Property(x => x.SettleAmountCur).HasPrecision(18, 4);
        b.Property(x => x.SettleAmountMst).HasPrecision(18, 4);
        b.Property(x => x.VendExchAdjustmentRealized).HasPrecision(18, 4);
    }
}
