using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Inventory;

/// <summary>A basic single-item, same-site transfer with independently posted shipment and receipt.</summary>
public class InventTransferOrder : Entity<long>
{
    [MaxLength(40)] public string TransferId { get; set; } = "";
    [MaxLength(20)] public string ItemId { get; set; } = "";
    [MaxLength(10)] public string Unit { get; set; } = "";
    [MaxLength(10)] public string SiteId { get; set; } = "";
    [MaxLength(10)] public string FromWarehouse { get; set; } = "";
    [MaxLength(10)] public string TransitWarehouse { get; set; } = "";
    [MaxLength(10)] public string ToWarehouse { get; set; } = "";
    [MaxLength(10)] public string JournalNameId { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal Shipped { get; set; }
    public decimal Received { get; set; }
}

public class InventTransferPosting : Entity<long>
{
    public long TransferOrderRecId { get; set; }
    [MaxLength(40)] public string RequestId { get; set; } = "";
    [MaxLength(20)] public string JournalId { get; set; } = "";
    public bool IsReceipt { get; set; }
    public decimal Quantity { get; set; }
    public DateTime PostingDate { get; set; }
    public decimal CostAmount { get; set; }
}

public class InventTransferOrderConfiguration : IEntityTypeConfiguration<InventTransferOrder>
{
    public void Configure(EntityTypeBuilder<InventTransferOrder> b)
    {
        b.ToTable("InventTransferOrder");
        b.HasIndex(x => new { x.DataAreaId, x.TransferId }).IsUnique();
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.Property(x => x.Shipped).HasPrecision(18, 4);
        b.Property(x => x.Received).HasPrecision(18, 4);
    }
}

public class InventTransferPostingConfiguration : IEntityTypeConfiguration<InventTransferPosting>
{
    public void Configure(EntityTypeBuilder<InventTransferPosting> b)
    {
        b.ToTable("InventTransferPosting");
        b.Property(x => x.CostAmount).HasPrecision(18, 4);
        b.HasIndex(x => new { x.DataAreaId, x.TransferOrderRecId, x.RequestId }).IsUnique();
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.HasOne<InventTransferOrder>().WithMany().HasForeignKey(x => x.TransferOrderRecId).OnDelete(DeleteBehavior.Restrict);
    }
}
