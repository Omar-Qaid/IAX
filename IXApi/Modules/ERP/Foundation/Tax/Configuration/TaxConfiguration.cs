using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.ERP.Shared.Features
{
    public class TaxDataConfiguration : IEntityTypeConfiguration<TaxData>
    {
        public void Configure(EntityTypeBuilder<TaxData> builder)
        {
            builder.ToTable("TaxData");

            // Primary Key
            builder.HasKey(x => x.RecId);

            // Relationships
            builder.HasOne(x => x.TaxTable)
                .WithMany()
                .HasForeignKey(x => x.TaxCode)
                .HasPrincipalKey(t => t.TaxCode)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class TaxGroupDataConfiguration : IEntityTypeConfiguration<TaxGroupData>
    {
        public void Configure(EntityTypeBuilder<TaxGroupData> builder)
        {
            builder.ToTable("TaxGroupData");

            // Primary Key
            builder.HasKey(x => x.RecId);

            // Relationships
            builder.HasOne(x => x.TaxTable)
                .WithMany()
                .HasForeignKey(x => x.TaxCode)
                .HasPrincipalKey(t => t.TaxCode)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TaxExemptCodeTable)
                .WithMany()
                .HasForeignKey(x => x.TaxExemptCode)
                .HasPrincipalKey(te => te.ExemptCode)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TaxGroupHeadingTable)
                .WithMany()
                .HasForeignKey(x => x.TaxGroup)
                .HasPrincipalKey(tg => tg.TaxGroup)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class TaxOnItemConfiguration : IEntityTypeConfiguration<TaxOnItem>
    {
        public void Configure(EntityTypeBuilder<TaxOnItem> builder)
        {
            builder.ToTable("TaxOnItem");

            // Primary Key
            builder.HasKey(x => x.RecId);

            // Relationships
            builder.HasOne(x => x.TaxTable)
                .WithMany()
                .HasForeignKey(x => x.TaxCode)
                .HasPrincipalKey(t => t.TaxCode)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TaxExemptCodeTable)
                .WithMany()
                .HasForeignKey(x => x.TaxExemptCode)
                .HasPrincipalKey(te => te.ExemptCode)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TaxItemGroupHeadingTable)
                .WithMany()
                .HasForeignKey(x => x.TaxItemGroup)
                .HasPrincipalKey(tig => tig.TaxItemGroup)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

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

    public class TaxAuthorityAddressConfiguration : IEntityTypeConfiguration<TaxAuthorityAddress>
    {
        public void Configure(EntityTypeBuilder<TaxAuthorityAddress> builder)
        {
            builder.ToTable("TaxAuthorityAddress");

            // Primary Key & Alternate Key
            builder.HasKey(x => x.RecId);
            builder.HasAlternateKey(x => x.TaxAuthority);

            // Relationships
            builder.HasOne(x => x.AddressLocation)
                .WithMany()
                .HasForeignKey(x => x.Location)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.VendTable)
                .WithMany()
                .HasForeignKey(x => x.AccountNum)
                .HasPrincipalKey(v => v.AccountNum)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RoundOffGainLedgerDimensionTable)
                .WithMany()
                .HasForeignKey(x => x.RoundOffGainLedgerDimension)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RoundOffLossLedgerDimensionTable)
                .WithMany()
                .HasForeignKey(x => x.RoundOffLossLedgerDimension)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class TaxPeriodHeadConfiguration : IEntityTypeConfiguration<TaxPeriodHead>
    {
        public void Configure(EntityTypeBuilder<TaxPeriodHead> builder)
        {
            builder.ToTable("TaxPeriodHead");

            // Primary Key
            builder.HasKey(x => x.RecId);

            // Relationships
            builder.HasOne(x => x.TaxAuthorityAddressTable)
                .WithMany()
                .HasForeignKey(x => x.TaxAuthority)
                .HasPrincipalKey(a => a.TaxAuthority)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class TaxReportPeriodConfiguration : IEntityTypeConfiguration<TaxReportPeriod>
    {
        public void Configure(EntityTypeBuilder<TaxReportPeriod> builder)
        {
            builder.ToTable("TaxReportPeriod");

            // Primary Key
            builder.HasKey(x => x.RecId);

            // Relationships
            builder.HasOne(x => x.TaxPeriodHeadTable)
                .WithMany()
                .HasForeignKey(x => x.TaxPeriod)
                .HasPrincipalKey(p => p.TaxPeriod)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class TaxGroupHeadingConfiguration : IEntityTypeConfiguration<TaxGroupHeading>
    {
        public void Configure(EntityTypeBuilder<TaxGroupHeading> builder)
        {
            builder.ToTable("TaxGroupHeading");

            // Primary Key
            builder.HasKey(x => x.RecId);
        }
    }

    public class TaxItemGroupHeadingConfiguration : IEntityTypeConfiguration<TaxItemGroupHeading>
    {
        public void Configure(EntityTypeBuilder<TaxItemGroupHeading> builder)
        {
            builder.ToTable("TaxItemGroupHeading");

            // Primary Key
            builder.HasKey(x => x.RecId);
        }
    }

    public class TaxLedgerAccountGroupConfiguration : IEntityTypeConfiguration<TaxLedgerAccountGroup>
    {
        public void Configure(EntityTypeBuilder<TaxLedgerAccountGroup> builder)
        {
            builder.ToTable("TaxLedgerAccountGroup");

            // Primary Key
            builder.HasKey(x => x.RecId);
        }
    }

    public class TaxTransConfiguration : IEntityTypeConfiguration<TaxTrans>
    {
        public void Configure(EntityTypeBuilder<TaxTrans> builder)
        {
            builder.ToTable("TaxTrans");

            // Primary Key
            builder.HasKey(x => x.RecId);
        }
    }

    public class TaxJournalTransConfiguration : IEntityTypeConfiguration<TaxJournalTrans>
    {
        public void Configure(EntityTypeBuilder<TaxJournalTrans> builder)
        {
            builder.ToTable("TaxJournalTrans");

            // Primary Key
            builder.HasKey(x => x.RecId);
        }
    }
}
