using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class PurchPrepayTable : Entity<long>
{
    public string PurchTable { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Type { get; set; }
    public long PrepayCategoryId { get; set; }
    public decimal Value { get; set; }
    public decimal PrepayLimit { get; set; }
    public decimal PrepayAvailable { get; set; }
    public decimal PrepayApplicationRemaining { get; set; }
}

public sealed class PurchPrepayTableConfiguration : IEntityTypeConfiguration<PurchPrepayTable>
{
    public void Configure(EntityTypeBuilder<PurchPrepayTable> builder)
    {
        builder.ToTable(nameof(PurchPrepayTable));
        builder.HasIndex(x => new { x.DataAreaId, x.PurchTable }).IsUnique();
        builder.Property(x => x.PurchTable).HasMaxLength(FieldLengths.PurchId);
        builder.Property(x => x.Currency).HasMaxLength(FieldLengths.CurrencyCode);
        builder.Property(x => x.Value).HasPrecision(18, 4);
        builder.Property(x => x.PrepayLimit).HasPrecision(18, 4);
        builder.Property(x => x.PrepayAvailable).HasPrecision(18, 4);
        builder.Property(x => x.PrepayApplicationRemaining).HasPrecision(18, 4);
    }
}
