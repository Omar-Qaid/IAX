using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.AccountsPayable.Configuration
{
    public class VendTableConfiguration : IEntityTypeConfiguration<VendTable>
    {
        public void Configure(EntityTypeBuilder<VendTable> builder)
        {
            builder.ToTable("VendTable");

            // Primary Key & Alternate Key
            builder.HasKey(x => x.RecId);
            builder.HasAlternateKey(x => x.AccountNum);

            // ==========================================================
            // Vendor -> Global Party
            // VendTable.Party -> DirPartyTable.RecId
            // ==========================================================
            builder.HasOne(x => x.DirPartyTable)
                   .WithMany()
                   .HasForeignKey(x => x.Party)
                   .HasPrincipalKey(x => x.RecId)
                   .OnDelete(DeleteBehavior.NoAction);

            // ==========================================================
            // Vendor -> Vendor Group
            // VendTable.VendGroup -> VendGroup.VendGroupCode
            // ==========================================================
            builder.HasOne(x => x.VendGroupTable)
                   .WithMany()
                   .HasForeignKey(x => x.VendGroup)
                   .HasPrincipalKey(x => x.VendGroupCode)
                   .OnDelete(DeleteBehavior.NoAction);

            // ==========================================================
            // Vendor -> Payment Terms
            // VendTable.PaymTermId -> PaymTerm.PaymTermId
            // ==========================================================
            builder.HasOne(x => x.PaymTermTable)
                   .WithMany()
                   .HasForeignKey(x => x.PaymTermId)
                   .HasPrincipalKey(x => x.PaymTermId)
                   .OnDelete(DeleteBehavior.NoAction);

            // ==========================================================
            // Vendor -> Main Contact Worker
            // VendTable.MainContactWorker -> HcmWorker.RecId
            // ==========================================================
            builder.HasOne(x => x.HcmWorkerTable)
                   .WithMany()
                   .HasForeignKey(x => x.MainContactWorker)
                   .HasPrincipalKey(x => x.RecId)
                   .OnDelete(DeleteBehavior.NoAction);

            // Business Keys / Indexes
            builder.HasIndex(x => new
            {
                x.AccountNum,
                x.DataAreaId
            }).IsUnique();

            builder.HasIndex(x => new
            {
                x.VendGroup,
                x.DataAreaId
            }).IsUnique(false);
        }
    }

    public class VendGroupConfiguration : IEntityTypeConfiguration<VendGroup>
    {
        public void Configure(EntityTypeBuilder<VendGroup> builder)
        {
            builder.ToTable("VendGroup");

            // Primary Key & Alternate Key
            builder.HasKey(x => x.RecId);
            builder.HasAlternateKey(x => x.VendGroupCode);

            // ==========================================================
            // Vendor Group -> Payment Terms
            // VendGroup.PaymTermId -> PaymTerm.PaymTermId
            // ==========================================================
            builder.HasOne(x => x.PaymTermTable)
                   .WithMany()
                   .HasForeignKey(x => x.PaymTermId)
                   .HasPrincipalKey(pt => pt.PaymTermId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => new
            {
                x.VendGroupCode,
                x.DataAreaId
            }).IsUnique();
        }
    }
}

