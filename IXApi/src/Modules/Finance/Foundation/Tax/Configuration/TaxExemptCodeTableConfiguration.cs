using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxExemptCodeTableConfiguration : IEntityTypeConfiguration<TaxExemptCodeTable>
{
    public void Configure(EntityTypeBuilder<TaxExemptCodeTable> builder)
    {
        builder.ToTable("TaxExemptCodeTable");
        builder.HasKey(row => row.RecId);
        builder.HasAlternateKey(row => row.ExemptCode);
        builder.Property(row => row.ExemptCode).HasMaxLength(10).IsRequired();
        builder.Property(row => row.Description).HasMaxLength(60).IsRequired();
    }
}
