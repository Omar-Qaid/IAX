using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.ERP.Foundation.Markup.Configuration
{
    public class MarkupTableConfiguration : IEntityTypeConfiguration<MarkupTable>
    {
        public void Configure(EntityTypeBuilder<MarkupTable> builder)
        {
            builder.ToTable("MarkupTable");

            // Primary Key
            builder.HasKey(x => x.RecId);

            // Alternate Key (Business Key)
            builder.HasAlternateKey(x => x.MarkupCode);

            // Properties
            builder.Property(x => x.MarkupCode)
                .IsRequired()
                .HasMaxLength(FieldLengths.MarkupCode);

            builder.Property(x => x.Txt)
                .IsRequired()
                .HasMaxLength(FieldLengths.Txt);

            builder.Property(x => x.TaxItemGroup)
                .IsRequired()
                .HasMaxLength(FieldLengths.TaxItemGroup);

            builder.Property(x => x.MaxAmount)
                .HasPrecision(18, 2);

            // Relationships
            builder.HasOne(x => x.CustomerDimensionAttributeValueCombinationTable)
                .WithMany()
                .HasForeignKey(x => x.CustomerLedgerDimension)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.VendorDimensionAttributeValueCombinationTable)
                .WithMany()
                .HasForeignKey(x => x.VendorLedgerDimension)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(x => x.MarkupCode)
                .IsUnique();
        }
    }

}
