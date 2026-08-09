using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class ExchangeRateCurrencyPairConfiguration : IEntityTypeConfiguration<ExchangeRateCurrencyPair>
    {
        public void Configure(EntityTypeBuilder<ExchangeRateCurrencyPair> builder)
        {
            builder.ToTable("ExchangeRateCurrencyPair");

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(4)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.FromCurrencyCode,
                x.ToCurrencyCode,
                x.ExchangeRateType,
                x.DataAreaId
            }).IsUnique();

            // From Currency
            builder.HasOne(x => x.FromCurrency)
                .WithMany(x => x.FromExchangeRateCurrencyPairs)
                .HasForeignKey(x => x.FromCurrencyCode)
                .HasPrincipalKey(x => x.CurrencyCode)
                .OnDelete(DeleteBehavior.Restrict);

            // To Currency
            builder.HasOne(x => x.ToCurrency)
                .WithMany(x => x.ToExchangeRateCurrencyPairs)
                .HasForeignKey(x => x.ToCurrencyCode)
                .HasPrincipalKey(x => x.CurrencyCode)
                .OnDelete(DeleteBehavior.Restrict);

            // Exchange Rate Type
            builder.HasOne(x => x.ExchangeRateTypeTable)
                .WithMany(x => x.ExchangeRateCurrencyPairs)
                .HasForeignKey(x => x.ExchangeRateType)
                .HasPrincipalKey(x => x.RecId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
