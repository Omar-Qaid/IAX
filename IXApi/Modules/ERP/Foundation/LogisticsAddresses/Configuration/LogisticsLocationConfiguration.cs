using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.ERP.Foundation.Features
{
    public class LogisticsLocationConfiguration : IEntityTypeConfiguration<LogisticsLocation>
    {
        public void Configure(EntityTypeBuilder<LogisticsLocation> builder)
        {
            builder.ToTable("LogisticsLocation");

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.LocationId)
                .HasMaxLength(FieldLengths.LocationId)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(FieldLengths.Description)
                .IsRequired();

            #region Parent Location (Self Reference)

            //builder.HasOne(x => x.LogisticsLocationParentTable)
            //    .WithMany()
            //    .HasForeignKey(x => x.ParentLocation)
            //    .HasPrincipalKey(x => x.RecId)
            //    .OnDelete(DeleteBehavior.Restrict);

            #endregion
        }
    }
}