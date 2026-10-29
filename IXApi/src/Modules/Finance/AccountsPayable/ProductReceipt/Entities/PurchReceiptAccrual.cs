using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class PurchReceiptAccrual : Entity<long>
{
    public long PackingSlipTransRecId { get; set; }
    public long GeneralJournalEntryRecId { get; set; }
    public long InventoryLedgerDimension { get; set; }
    public long AccrualLedgerDimension { get; set; }
    public decimal InventQuantity { get; set; }
    public decimal AmountMst { get; set; }
    public decimal ReversedInventQuantity { get; set; }
    public decimal ReversedAmountMst { get; set; }
}

public class PurchReceiptAccrualConfiguration : IEntityTypeConfiguration<PurchReceiptAccrual>
{
    public void Configure(EntityTypeBuilder<PurchReceiptAccrual> b)
    {
        b.ToTable("PurchReceiptAccrual");
        b.HasIndex(x => new { x.DataAreaId, x.PackingSlipTransRecId }).IsUnique();
        b.HasOne<VendPackingSlipTrans>().WithMany().HasForeignKey(x => x.PackingSlipTransRecId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<GeneralJournalEntry>().WithMany().HasForeignKey(x => x.GeneralJournalEntryRecId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<DimensionAttributeValueCombination>().WithMany().HasForeignKey(x => x.InventoryLedgerDimension).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<DimensionAttributeValueCombination>().WithMany().HasForeignKey(x => x.AccrualLedgerDimension).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.InventQuantity).HasPrecision(18, 6);
        b.Property(x => x.ReversedInventQuantity).HasPrecision(18, 6);
        b.Property(x => x.AmountMst).HasPrecision(18, 4);
        b.Property(x => x.ReversedAmountMst).HasPrecision(18, 4);
    }
}
