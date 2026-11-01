using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.CashDiscount;

public sealed class CashDiscConfiguration : IEntityTypeConfiguration<CashDisc>
{
    public void Configure(EntityTypeBuilder<CashDisc> b)
    {
        b.ToTable("CashDisc");
        b.HasKey(x => x.RecId);
        b.HasIndex(x => new { x.DataAreaId, x.CashDiscCode }).IsUnique().HasFilter("[IsDeleted] = 0");
        b.Property(x => x.DiscPct).HasPrecision(18, 6);
    }
}
