using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
{
    public class ContactPersonConfiguration : IEntityTypeConfiguration<ContactPerson>
    {
        public void Configure(EntityTypeBuilder<ContactPerson> builder)
        {
            builder.ToTable("ContactPerson");

            // Primary Key
            builder.HasKey(x => x.RecId);

            // Business Key
            builder.HasIndex(x => new { x.ContactPersonId, x.DataAreaId }).IsUnique();

            // Properties
            builder.Property(x => x.ContactPersonId)
                .HasMaxLength(FieldLengths.ContactPersonId)
                .IsRequired();

            builder.Property(x => x.CustAccount)
                .HasMaxLength(FieldLengths.CustAccount)
                .IsRequired();

            builder.Property(x => x.DataAreaId)
                .HasDefaultValue("dat");

            // Relationships

            // ContactPerson.Party == DirPartyTable.RecId
            builder.HasOne(x => x.DirPartyTable)
                .WithMany()
                .HasForeignKey(x => x.Party)
                .OnDelete(DeleteBehavior.Restrict);

            // ContactPerson.ContactForParty == DirPartyTable.RecId
            builder.HasOne<DirPartyTable>()
                .WithMany()
                .HasForeignKey(x => x.ContactForParty)
                .OnDelete(DeleteBehavior.Restrict);

            // ContactPerson.CustAccount == CustTable.AccountNum
            builder.HasOne<CustTable>()
                .WithMany()
                .HasForeignKey(x => x.CustAccount)
                .HasPrincipalKey(x => x.AccountNum)
                .OnDelete(DeleteBehavior.Restrict);

            // ContactPerson.MainResponsibleWorker == HcmWorker.RecId
            builder.HasOne<HcmWorker>()
                .WithMany()
                .HasForeignKey(x => x.MainResponsibleWorker)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

