using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class TaxTableConfiguration : IEntityTypeConfiguration<TaxTable>
    {
        public void Configure(EntityTypeBuilder<TaxTable> builder)
        {
            builder.ToTable("TaxTable");

            // Primary Key
            builder.HasKey(x => x.RecId);

            // Relationships
            builder.HasOne(x => x.Currency)
                .WithMany()
                .HasForeignKey(x => x.TaxCurrencyCode)
                .HasPrincipalKey(c => c.CurrencyCode)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.UnitOfMeasureTable)
                .WithMany()
                .HasForeignKey(x => x.TaxUnit)
                .HasPrincipalKey(u => u.Symbol)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
