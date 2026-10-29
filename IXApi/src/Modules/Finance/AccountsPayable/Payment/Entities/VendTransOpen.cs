using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendTransOpen : Entity<long>
{
    [MaxLength(20)] public string AccountNum { get; set; } = "";
    public long RefRecId { get; set; }
    public DateTime TransDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal AmountCur { get; set; }
    public decimal AmountMst { get; set; }
}

public class VendSettlement : Entity<long>
{
    [MaxLength(20)] public string AccountNum { get; set; } = "";
    public long InvoiceTransRecId { get; set; }
    public long PaymentTransRecId { get; set; }
    public DateTime TransDate { get; set; }
    [MaxLength(20)] public string Voucher { get; set; } = "";
    public decimal AmountCur { get; set; }
    public decimal AmountMst { get; set; }
}

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
        b.HasIndex(x => new { x.DataAreaId, x.InvoiceTransRecId, x.PaymentTransRecId }).IsUnique();
        b.HasOne<VendTrans>().WithMany().HasForeignKey(x => x.InvoiceTransRecId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<VendTrans>().WithMany().HasForeignKey(x => x.PaymentTransRecId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.AmountCur).HasPrecision(18, 4);
        b.Property(x => x.AmountMst).HasPrecision(18, 4);
    }
}
