using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
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
