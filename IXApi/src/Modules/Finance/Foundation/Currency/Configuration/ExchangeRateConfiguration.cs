using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
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
}
