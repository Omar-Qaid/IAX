using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace IAX.IXApi.Modules.ERP.Shared.Features
{
    public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
    {
        public void Configure(EntityTypeBuilder<Currency> builder)
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

    public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
    {
        public void Configure(EntityTypeBuilder<ExchangeRate> builder)
        {
            builder.ToTable("ExchangeRate");

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();

            builder.HasOne(x => x.ExchangeRateCurrencyPairTable)
           .WithMany(x => x.ExchangeRates)
           .HasForeignKey(x => x.ExchangeRateCurrencyPair)
           .HasPrincipalKey(x => x.RecId)
           .OnDelete(DeleteBehavior.Restrict);
        }
    }

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
    public class ExchangeRateTypeConfiguration : IEntityTypeConfiguration<ExchangeRateType>
    {
        public void Configure(EntityTypeBuilder<ExchangeRateType> builder)
        {
            builder.ToTable("ExchangeRateType");
            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();

            builder.HasKey(x => x.RecId);

            builder.HasAlternateKey(x => x.Name);

            builder.HasIndex(x => new { x.Name, x.DataAreaId }).IsUnique();

        }
    }
}

