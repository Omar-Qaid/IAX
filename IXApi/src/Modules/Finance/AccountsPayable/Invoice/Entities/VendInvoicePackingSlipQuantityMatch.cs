using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendInvoicePackingSlipQuantityMatch : Entity<long>
{
    public long InvoiceTransRecId { get; set; }
    public long PackingSlipTransRecId { get; set; }
    public decimal Quantity { get; set; }
    public decimal InventQuantity { get; set; }
    public long? ReceiptAccrualRecId { get; set; }
    public decimal AccrualReversalAmountMst { get; set; }
}

public class VendInvoicePackingSlipQuantityMatchConfiguration : IEntityTypeConfiguration<VendInvoicePackingSlipQuantityMatch>
{
    public void Configure(EntityTypeBuilder<VendInvoicePackingSlipQuantityMatch> b)
    {
        b.ToTable("VendInvoicePackingSlipQuantityMatch");
        b.HasKey(x => x.RecId);
        b.HasIndex(x => new { x.DataAreaId, x.InvoiceTransRecId, x.PackingSlipTransRecId }).IsUnique();
        b.HasOne<VendInvoiceTrans>().WithMany().HasForeignKey(x => x.InvoiceTransRecId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<VendPackingSlipTrans>().WithMany().HasForeignKey(x => x.PackingSlipTransRecId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Quantity).HasPrecision(18, 6);
        b.Property(x => x.InventQuantity).HasPrecision(18, 6);
        b.Property(x => x.AccrualReversalAmountMst).HasPrecision(18, 4);
        b.HasOne<PurchReceiptAccrual>().WithMany().HasForeignKey(x => x.ReceiptAccrualRecId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PurchaseInvoiceTaxReferenceConfiguration : IEntityTypeConfiguration<VendInvoiceTrans>
{
    public void Configure(EntityTypeBuilder<VendInvoiceTrans> b)
    {
        b.HasOne<VendInvoiceTrans>().WithMany().HasForeignKey(x => x.OriginalInvoiceTransRecId).OnDelete(DeleteBehavior.Restrict);
    }
}
