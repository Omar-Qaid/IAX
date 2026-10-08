using CurrencyEntity = IAX.IXApi.Modules.Finance.Entities.Currency;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace IAX.IXApi.Modules.Finance.Foundation.Currency
{
    public class CurrencyConfiguration : IEntityTypeConfiguration<CurrencyEntity>
    {
        public void Configure(EntityTypeBuilder<CurrencyEntity> builder)
        {
            builder.ToTable("Currency");

            builder.HasKey(x => x.RecId);
            builder.HasIndex(x => new { x.CurrencyCode, x.DataAreaId }).IsUnique();

            builder.HasAlternateKey(x => x.CurrencyCode);

            builder.Property(x => x.CurrencyCode)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.Symbol)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(4)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => x.CurrencyCode).IsUnique();
            builder.HasIndex(x => x.DataAreaId);
        }
    }
}
