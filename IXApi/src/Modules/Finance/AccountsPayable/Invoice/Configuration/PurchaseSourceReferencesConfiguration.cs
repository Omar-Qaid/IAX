using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace IAX.IXApi.Modules.Finance.AccountsPayable;
public sealed class PurchTableSourceConfiguration : IEntityTypeConfiguration<PurchTable>
{
    public void Configure(EntityTypeBuilder<PurchTable> b) => b.HasOne<SourceDocumentHeader>().WithMany().HasForeignKey(x => x.SourceDocumentHeader).OnDelete(DeleteBehavior.Restrict);
}
public sealed class PurchLineSourceConfiguration : IEntityTypeConfiguration<PurchLine>
{
    public void Configure(EntityTypeBuilder<PurchLine> b) => b.HasOne<SourceDocumentLine>().WithMany().HasForeignKey(x => x.SourceDocumentLine).OnDelete(DeleteBehavior.Restrict);
}
public sealed class VendInvoiceInfoSourceConfiguration : IEntityTypeConfiguration<VendInvoiceInfoTable>
{
    public void Configure(EntityTypeBuilder<VendInvoiceInfoTable> b) => b.HasOne<SourceDocumentHeader>().WithMany().HasForeignKey(x => x.SourceDocumentHeader).OnDelete(DeleteBehavior.Restrict);
}
public sealed class VendInvoiceInfoLineSourceConfiguration : IEntityTypeConfiguration<VendInvoiceInfoLine>
{
    public void Configure(EntityTypeBuilder<VendInvoiceInfoLine> b) => b.HasOne<SourceDocumentLine>().WithMany().HasForeignKey(x => x.SourceDocumentLine).OnDelete(DeleteBehavior.Restrict);
}
public sealed class VendInvoiceJourSourceConfiguration : IEntityTypeConfiguration<VendInvoiceJour>
{
    public void Configure(EntityTypeBuilder<VendInvoiceJour> b) => b.HasOne<SourceDocumentHeader>().WithMany().HasForeignKey(x => x.SourceDocumentHeader).OnDelete(DeleteBehavior.Restrict);
}
public sealed class VendInvoiceTransSourceConfiguration : IEntityTypeConfiguration<VendInvoiceTrans>
{
    public void Configure(EntityTypeBuilder<VendInvoiceTrans> b) => b.HasOne<SourceDocumentLine>().WithMany().HasForeignKey(x => x.SourceDocumentLine).OnDelete(DeleteBehavior.Restrict);
}
public sealed class VendPackingSlipJourSourceConfiguration : IEntityTypeConfiguration<VendPackingSlipJour>
{
    public void Configure(EntityTypeBuilder<VendPackingSlipJour> b) => b.HasOne<SourceDocumentHeader>().WithMany().HasForeignKey(x => x.SourceDocumentHeader).OnDelete(DeleteBehavior.Restrict);
}
public sealed class VendPackingSlipTransSourceConfiguration : IEntityTypeConfiguration<VendPackingSlipTrans>
{
    public void Configure(EntityTypeBuilder<VendPackingSlipTrans> b) => b.HasOne<SourceDocumentLine>().WithMany().HasForeignKey(x => x.SourceDocumentLine).OnDelete(DeleteBehavior.Restrict);
}
